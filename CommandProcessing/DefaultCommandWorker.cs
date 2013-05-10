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
    using CommandProcessing.Tasks;
    using CommandProcessing.Validation;

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

        public TResult Execute<TCommand, TResult>(HandlerRequest request) where TCommand : ICommand
        {
            IHandlerSelector handlerSelector = this.Configuration.Services.GetHandlerSelector();
            HandlerDescriptor descriptor = handlerSelector.SelectHandler(request);

            IHandler handler = descriptor.CreateHandler(request);

            if (handler == null)
            {
                throw new HandlerNotFoundException(typeof(TCommand));
            }

            if (!this.ValidateCommand(request) && this.Configuration.AbortOnInvalidCommand)
            {
                return default(TResult);
            }

            HandlerContext context = new HandlerContext(request, descriptor);
            context.User = this.Configuration.Services.GetPrincipalProvider().Principal;
            handler.Context = context;

            FilterGrouping filterGrouping = descriptor.GetFilterGrouping();
            CancellationToken cancellationToken = new CancellationToken();
            
            Func<Task<object>> invokeFunc = InvokeHandlerWithHandlerFiltersAsync(context, cancellationToken, filterGrouping.HandlerFilters, () => InvokeHandlerAsync(context, handler, cancellationToken));
            Task<object> result = invokeFunc();
                
            result = InvokeHandlerWithExceptionFiltersAsync(result, context, cancellationToken, filterGrouping.ExceptionFilters);

            return (TResult)result.Result;
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
                    Task<TResult> resultTask =
                        TaskHelpers.Iterate(lazyTaskEnumeration, cancellationToken)
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

        private static Func<Task<TResult>> InvokeHandlerWithHandlerFiltersAsync<TResult>(HandlerContext context, CancellationToken cancellationToken, IEnumerable<IHandlerFilter> filters, Func<Task<TResult>> innerAction)
        {
            Contract.Requires(context != null);
            Contract.Requires(filters != null);
            Contract.Requires(innerAction != null);

            // Because the continuation gets built from the inside out we need to reverse the filter list
            // so that least specific filters (Global) get run first and the most specific filters (Handler) get run last.
            filters = filters.Reverse();

            Func<Task<TResult>> result = filters.Aggregate(
                innerAction,
                (continuation, filter) => () => filter.ExecuteHandlerFilterAsync(context, cancellationToken, continuation));

            return result;
        }

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
    }
}