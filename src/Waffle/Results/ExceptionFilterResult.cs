namespace Waffle.Results
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.ExceptionHandling;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Properties;

    internal class ExceptionFilterResult : ICommandHandlerResult
    {
        private readonly CommandHandlerContext context;
        private readonly IExceptionFilter[] filters;
        private readonly IExceptionLogger exceptionLogger;
        private readonly IExceptionHandler exceptionHandler;

        private readonly ICommandHandlerResult innerResult;

        public ExceptionFilterResult(CommandHandlerContext context, IExceptionFilter[] filters, IExceptionLogger exceptionLogger, IExceptionHandler exceptionHandler, ICommandHandlerResult innerResult)
        {
            Contract.Requires(context != null);
            Contract.Requires(filters != null);
            Contract.Requires(exceptionLogger != null);
            Contract.Requires(exceptionHandler != null);
            Contract.Requires(innerResult != null);

            this.context = context;
            this.filters = filters;
            this.exceptionLogger = exceptionLogger;
            this.exceptionHandler = exceptionHandler;
            this.innerResult = innerResult;
        }

        public async Task<HandlerResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            ExceptionDispatchInfo exceptionInfo;

            try
            {
                return await this.innerResult.ExecuteAsync(cancellationToken);
            }
            catch (Exception e)
            {
                exceptionInfo = ExceptionDispatchInfo.Capture(e);
            }

            // This code path only runs if the task is faulted with an exception
            Exception exception = exceptionInfo.SourceException;
            bool isCancellationException = exception is OperationCanceledException;

            ExceptionContext exceptionContext = new ExceptionContext(exceptionInfo, ExceptionCatchBlocks.ExceptionFilter, this.context);

            if (!isCancellationException)   
            {   
                // We don't log cancellation exceptions because it doesn't represent an error.   
                await this.exceptionLogger.LogAsync(exceptionContext, cancellationToken);   
            }  
            
            CommandHandlerExecutedContext executedContext = new CommandHandlerExecutedContext(this.context, exceptionInfo);

            // Note: exception filters need to be scheduled in the reverse order so that
            // the more specific filter (e.g. Action) executes before the less specific ones (e.g. Global)
            for (int i = this.filters.Length - 1; i >= 0; i--)
            {
                IExceptionFilter exceptionFilter = this.filters[i];
                await exceptionFilter.ExecuteExceptionFilterAsync(executedContext, cancellationToken);
            }

            if (executedContext.Response == null && !isCancellationException)
            {
                // We don't log cancellation exceptions because it doesn't represent an error. 
                executedContext.Response = await this.exceptionHandler.HandleAsync(exceptionContext, cancellationToken);
            }

            if (executedContext.Response != null)
            {
                return executedContext.Response;
            }

            // Preserve the original stack trace when the exception is not changed by any filter.
            if (exception == executedContext.ExceptionInfo.SourceException)
            {
                exceptionInfo.Throw();
            }
            else
            {
                // If the exception is changed by a filter, throw the new exception instead.
                executedContext.ExceptionInfo.Throw();
            }

            throw Error.InvalidOperation(Resources.UnreachableCode);
        }
    }
}
