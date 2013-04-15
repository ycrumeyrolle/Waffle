namespace CommandProcessing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;
    using CommandProcessing.Services;
    using CommandProcessing.Tasks;
    using CommandProcessing.Validation;

    /// <summary>
    /// Represents a processor of commands. 
    /// Its role is to take a command from a client, validate it, and delegate the processing to an handler.
    /// Then it returns the result to the client.
    /// </summary>
    public class CommandProcessor : ICommandProcessor, IDisposable
    {
        private bool disposed;

        private IPrincipalProvider principalProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandProcessor"/> class. 
        /// </summary>
        public CommandProcessor()
            : this(new ProcessorConfiguration())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandProcessor"/> class. 
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public CommandProcessor(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            this.Configuration = configuration;

            this.Initialize();
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public ProcessorConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets the handler selector.
        /// </summary>
        /// <value>The configuration.</value>
        public IHandlerSelector HandlerSelector { get; private set; }
        
        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <typeparam name="TCommand">The type of command to process.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="command">The command to process.</param>
        /// <param name="currentRequest">The current request. Pass null if there is not parent request.</param>
        /// <returns>The result of the command.</returns>
        public TResult Process<TCommand, TResult>(TCommand command, HandlerRequest currentRequest) where TCommand : ICommand
        {
            using (HandlerRequest request = new HandlerRequest(this.Configuration, command, currentRequest))
            {
                HandlerDescriptor descriptor = this.HandlerSelector.SelectHandler(request);

                Handler handler = descriptor.CreateHandler(request);

                if (handler == null)
                {
                    throw new HandlerNotFoundException(typeof(TCommand));
                }

                if (!this.ValidateCommand(command) && this.Configuration.AbortOnInvalidCommand)
                {
                    return default(TResult);
                }

                HandlerContext context = new HandlerContext(request, descriptor);
                context.User = this.principalProvider.Principal;
                handler.Context = context;
                handler.Processor = this;

                ICollection<FilterInfo> filterPipeline = descriptor.GetFilterPipeline();

                FilterGrouping filterGrouping = new FilterGrouping(filterPipeline);
                CancellationToken cancellationToken = new CancellationToken();

                Task<TResult> result = TaskHelpers.RunSynchronously(() =>
                {
                    Func<Task<TResult>> invokeFunc = InvokeHandlerWithHandlerFiltersAsync(context, cancellationToken, filterGrouping.HandlerFilters, () =>
                    {
                        return InvokeHandlerAsync<TResult>(context, handler, cancellationToken);
                    });
                    return invokeFunc();
                });

                result = InvokeActionWithExceptionFiltersAsync(result, context, cancellationToken, filterGrouping.ExceptionFilters);

                return result.Result;
            }
        }

        private bool ValidateCommand(ICommand command)
        {
            IEnumerable<ICommandValidator> validators = this.Configuration.Services.GetCommandValidators();
            bool valid = validators
                .Select(validator => validator.Validate(command))
                .Aggregate(true, (current, commandValid) => current & commandValid);

            return valid;
        }

        /// <summary>
        /// Asks the the processor to supply a service.
        /// The service will be created by the <see cref="IDependencyResolver"/>.
        /// If the ServiceProxyCreationEnabled is <c>true</c>, the service will be a proxy.
        /// </summary>
        /// <typeparam name="TService">The type of the service to supply.</typeparam>
        /// <returns>The service.</returns>
        public TService Using<TService>() where TService : class
        {
            var service = this.Configuration.DependencyResolver.GetServiceOrThrow<TService>();

            if (this.Configuration.ServiceProxyCreationEnabled)
            {
                var proxyBuilder = this.Configuration.Services.GetProxyBuilder();
                var interceptorProvider = this.Configuration.Services.GetInterceptorProvider();

                service = proxyBuilder.Build(service, interceptorProvider);
            }

            return service;
        }

        /// <summary>
        /// Releases the unmanaged resources that are used by the object and releases the managed resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal static Task<TResult> InvokeActionWithExceptionFiltersAsync<TResult>(Task<TResult> actionTask, HandlerContext context, CancellationToken cancellationToken, IEnumerable<IExceptionFilter> filters)
        {
            Contract.Assert(actionTask != null);
            Contract.Assert(context != null);
            Contract.Assert(filters != null);

            return actionTask.Catch(
                info =>
                {
                    HandlerExecutedContext executedContext = new HandlerExecutedContext(context, info.Exception);

                    // Note: exception filters need to be scheduled in the reverse order so that
                    // the more specific filter (e.g. Action) executes before the less specific ones (e.g. Global)
                    filters = filters.Reverse();

                    // Note: in order to work correctly with the TaskHelpers.Iterate method, the lazyTaskEnumeration
                    // must be lazily evaluated. Otherwise all the tasks might start executing even though we want to run them
                    // sequentially and not invoke any of the following ones if an earlier fails.
                    IEnumerable<Task> lazyTaskEnumeration = filters.Select(filter => filter.ExecuteExceptionFilterAsync(executedContext, cancellationToken));
                    Task<TResult> resultTask =
                        TaskHelpers.Iterate(lazyTaskEnumeration, cancellationToken)
                                   .Then(() =>
                                       {
                                           if (executedContext.Result != null)
                                           {
                                               return TaskHelpers.FromResult((TResult)executedContext.Result);
                                           }

                                           return TaskHelpers.FromError<TResult>(executedContext.Exception);
                                       }, runSynchronously: true);

                    return info.Task(resultTask);
                });
        }
        
        internal static Func<Task<TResult>> InvokeHandlerWithHandlerFiltersAsync<TResult>(HandlerContext context, CancellationToken cancellationToken, IEnumerable<IHandlerFilter> filters, Func<Task<TResult>> innerAction)
        {
            Contract.Assert(context != null);
            Contract.Assert(filters != null);
            Contract.Assert(innerAction != null);

            // Because the continuation gets built from the inside out we need to reverse the filter list
            // so that least specific filters (Global) get run first and the most specific filters (Handler) get run last.
            filters = filters.Reverse();

            Func<Task<TResult>> result = filters.Aggregate(innerAction, (continuation, filter) =>
            {
                return () => filter.ExecuteHandlerFilterAsync(context, cancellationToken, continuation);
            });

            return result;
        }
        
        internal static Task<TResult> InvokeHandlerAsync<TResult>(HandlerContext context, Handler handler, CancellationToken cancellationToken)
        {
            return TaskHelpers.RunSynchronously(
                () =>
                    {
                        TResult result = (TResult)handler.Handle(context.Command);
                        return TaskHelpers.FromResult(result);
                    },
                cancellationToken);
        }
        
        /// <summary>
        /// Releases the unmanaged resources that are used by the object and, optionally, releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;
                if (disposing)
                {
                    this.Configuration.Dispose();
                }
            }
        }

        private void Initialize()
        {
            this.Configuration.Initializer(this.Configuration);
            this.Configuration.Services.Replace(typeof(ICommandProcessor), this);
            this.HandlerSelector = this.Configuration.Services.GetHandlerSelector();
            this.principalProvider = this.Configuration.Services.GetPrincipalProvider();
        }
    }
}