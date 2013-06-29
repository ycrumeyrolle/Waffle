namespace CommandProcessing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;
    using CommandProcessing.Tasks;
    using CommandProcessing.Validation;

    /// <summary>
    /// Default implementation of the <see cref="ICommandWorker"/>.
    /// </summary>
    public sealed class DefaultCommandWorker : ICommandWorker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandWorker"/> class.
        /// </summary>
        /// <param name="configuration"></param>
        public DefaultCommandWorker(ProcessorConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public ProcessorConfiguration Configuration { get; private set; }

        /// <summary>
        /// Execute the request via the worker. 
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="request">The <see cref="HandlerRequest"/> to execute.</param>
        /// <returns>The result of the command, if any.</returns>
        public Task<TResult> ExecuteAsync<TResult>(HandlerRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            IHandlerSelector handlerSelector = this.Configuration.Services.GetHandlerSelector();
            HandlerDescriptor descriptor = handlerSelector.SelectHandler(request);

            IHandler handler = descriptor.CreateHandler(request);

            if (handler == null)
            {
                throw CreateHandlerNotFoundException(descriptor);
            }

            this.RegisterForDispose(request, descriptor.Lifetime, handler);

            if (!this.ValidateCommand(request) && this.Configuration.AbortOnInvalidCommand)
            {
                return TaskHelpers.Completed<TResult>();
            }

            HandlerContext context = new HandlerContext(request, descriptor);
            handler.Context = context;

            FilterGrouping filterGrouping = descriptor.GetFilterGrouping();
            CancellationToken cancellationToken = new CancellationToken();

            Func<Task<object>> invokeFunc = InvokeHandlerWithHandlerFiltersAsync(context, cancellationToken, filterGrouping.HandlerFilters, () => InvokeHandlerAsync(context, handler, cancellationToken));
            Task<object> result = invokeFunc();

            result = InvokeHandlerWithExceptionFiltersAsync(result, context, cancellationToken, filterGrouping.ExceptionFilters);

            return result.CastFromObject<TResult>();
        }

        private void RegisterForDispose(HandlerRequest request, HandlerLifetime lifetime, IHandler handler)
        {
            if (lifetime == HandlerLifetime.Processor)
            {
                // Per-processor lifetime will be disposed only on processor disposing
                this.Configuration.RegisterForDispose(handler as IDisposable);
            }
            else
            {
                // Per-request and transcient lifetime will be disposed on main request disposing
                request.RegisterForDispose(handler, true);
            }
        }

        private bool ValidateCommand(HandlerRequest request)
        {
            ICommandValidator validator = this.Configuration.Services.GetCommandValidator();
            bool valid = validator.Validate(request);

            return valid;
        }

        private static Task<TResult> InvokeHandlerWithExceptionFiltersAsync<TResult>(Task<TResult> task, HandlerContext context, CancellationToken cancellationToken, IEnumerable<IExceptionFilter> filters)
        {
            Contract.Requires(task != null);
            Contract.Requires(context != null);
            Contract.Requires(filters != null);

            return task.Catch(
                info =>
                {
                    HandlerExecutedContext executedContext = new HandlerExecutedContext(context, info.Exception);

                    // Note: exception filters need to be scheduled in the reverse order so that
                    // the more specific filter (e.g. Handler) executes before the less specific ones (e.g. Global)
                    filters = filters.Reverse();

                    // Note: in order to work correctly with the TaskHelpers.Iterate method, the lazyTaskEnumeration
                    // must be lazily evaluated. Otherwise all the tasks might start executing even though we want to run them
                    // sequentially and not invoke any of the following ones if an earlier fails.
                    IEnumerable<Task> lazyTaskEnumeration = filters.Select(filter => filter.ExecuteExceptionFilterAsync(executedContext, cancellationToken));
                    Task<TResult> resultTask = TaskHelpers.Iterate(lazyTaskEnumeration, cancellationToken)
                                   .Then(
                                   () =>
                                   {
                                       if (executedContext.Result != null)
                                       {
                                           return TaskHelpers.FromResult((TResult)executedContext.Result);
                                       }

                                       return TaskHelpers.FromError<TResult>(executedContext.Exception);
                                   },
                                    runSynchronously: true);

                    return info.Task(resultTask);
                });
        }

        private static Func<Task<TResult>> InvokeHandlerWithHandlerFiltersAsync<TResult>(HandlerContext context, CancellationToken cancellationToken, IHandlerFilter[] filters, Func<Task<TResult>> innerAction)
        {
            Contract.Requires(context != null);
            Contract.Requires(filters != null);
            Contract.Requires(innerAction != null);

            Func<Task<TResult>> result = innerAction;
            for (int i = filters.Length - 1; i >= 0; i--)
            {
                IHandlerFilter filter = filters[i];
                Func<Func<Task<TResult>>, IHandlerFilter, Func<Task<TResult>>> chainContinuation = (continuation, innerFilter) =>
                {
                    return () => innerFilter.ExecuteHandlerFilterAsync(context, cancellationToken, continuation);
                };

                result = chainContinuation(result, filter);
            }

            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        private static Task<object> InvokeHandlerAsync(HandlerContext context, IHandler handler, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskHelpers.Canceled<object>();
            }

            try
            {
                var res = handler.Handle(context.Command);
                return TaskHelpers.FromResult(res);
            }
            catch (Exception e)
            {
                return TaskHelpers.FromError<object>(e);
            }
        }

        private static HandlerNotFoundException CreateHandlerNotFoundException(HandlerDescriptor descriptor)
        {
            if (descriptor.ResultType == typeof(VoidResult))
            {
                return new HandlerNotFoundException(descriptor.CommandType);
            }

            return new HandlerNotFoundException(descriptor.CommandType, descriptor.ResultType);
        }
    }
}