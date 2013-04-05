namespace CommandProcessing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;
    using CommandProcessing.Services;
    using CommandProcessing.Tasks;
    using CommandProcessing.Validation;

    public class CommandProcessor : ICommandProcessor, IDisposable
    {
        private bool disposed;

        public CommandProcessor()
            : this(new ProcessorConfiguration())
        {
        }

        public CommandProcessor(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            this.Configuration = configuration;

            this.Initialize();
        }

        public ProcessorConfiguration Configuration { get; private set; }

        public IHandlerSelector HandlerSelector { get; private set; }
        
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
                handler.Context = context;

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
            // so that least specific filters (Global) get run first and the most specific filters (Action) get run last.
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
            this.HandlerSelector = this.Configuration.Services.GetHandlerSelector();
        }
    }
}