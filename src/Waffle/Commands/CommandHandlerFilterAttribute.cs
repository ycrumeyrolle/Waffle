namespace Waffle.Commands
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Properties;
    using Waffle.Tasks;

    /// <summary>
    /// Represents the base class for all handler-filter attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class CommandHandlerFilterAttribute : FilterAttribute, ICommandHandlerFilter
    {
        /// <summary>
        /// Occurs before the handle method is invoked.
        /// </summary>
        /// <param name="handlerContext">The handler context.</param>
        /// <remarks>
        /// Overrides this method to add a behaviour before a command in a non-asynchronous way.
        /// </remarks>
        public virtual void OnCommandExecuting(CommandHandlerContext handlerContext)
        {
        }

        /// <summary>
        /// Occurs after the handle method is invoked.
        /// </summary>
        /// <param name="handlerExecutedContext">The handler executed context.</param>
        /// <remarks>
        /// Overrides this method to add a behaviour after a command in a non-asynchronous way.
        /// </remarks>
        public virtual void OnCommandExecuted(CommandHandlerExecutedContext handlerExecutedContext)
        {
        }

        /// <summary>
        /// Occurs before the handle method is invoked.
        /// </summary>
        /// <param name="handlerContext">The handler context.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <remarks>
        /// Overrides this method to add a behaviour before a command in a asynchronous way.
        /// </remarks>
       [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "exception is flowed through the task")]
        public virtual Task OnCommandExecutingAsync(CommandHandlerContext handlerContext, CancellationToken cancellationToken)
        {
            try
            {
                this.OnCommandExecuting(handlerContext);
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError(ex);
            }

            return TaskHelpers.Completed();
        }

        /// <summary>
        /// Occurs after the handle method is invoked.
        /// </summary>
        /// <param name="handlerExecutedContext">The handler context.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
       /// <remarks>
       /// Overrides this method to add a behaviour after a command in a asynchronous way.
       /// </remarks>
       [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "exception is flowed through the task")]
        public virtual Task OnCommandExecutedAsync(CommandHandlerExecutedContext handlerExecutedContext, CancellationToken cancellationToken)
        {
            try
            {
                this.OnCommandExecuted(handlerExecutedContext);
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError(ex);
            }

            return TaskHelpers.Completed();
        }
        
        /// <summary>
        /// Executes the filter handler asynchronously.
        /// </summary>
        /// <param name="handlerContext">The handler context.</param>
        /// <param name="cancellationToken">The cancellation token assigned for this task.</param>
        /// <param name="continuation">The delegate function to continue after the handler method is invoked.</param>
        /// <returns>The newly created task for this operation.</returns>
        public Task<HandlerResponse> ExecuteHandlerFilterAsync(CommandHandlerContext handlerContext, CancellationToken cancellationToken, Func<Task<HandlerResponse>> continuation)
        {
            if (handlerContext == null)
            {
                throw Error.ArgumentNull("handlerContext");
            }

            if (continuation == null)
            {
                throw Error.ArgumentNull("continuation");
            }
            
            return this.ExecuteHandlerFilterAsyncCore(handlerContext, cancellationToken, continuation);
        }

        private async Task<HandlerResponse> ExecuteHandlerFilterAsyncCore(CommandHandlerContext handlerContext, CancellationToken cancellationToken, Func<Task<HandlerResponse>> continuation)
        {
            await this.OnCommandExecutingAsync(handlerContext, cancellationToken);

            if (handlerContext.Response != null)
            {
                return handlerContext.Response;
            }

            return await this.CallOnHandlerExecutedAsync(handlerContext, cancellationToken, continuation);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to intercept all exceptions")]
        private async Task<HandlerResponse> CallOnHandlerExecutedAsync(CommandHandlerContext handlerContext, CancellationToken cancellationToken, Func<Task<HandlerResponse>> continuation)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HandlerResponse response = null;
            ExceptionDispatchInfo exceptionInfo = null;
            try
            {
                response = await continuation();
            }
            catch (Exception e)
            {
                exceptionInfo = ExceptionDispatchInfo.Capture(e);
            }

            CommandHandlerExecutedContext executedContext = new CommandHandlerExecutedContext(handlerContext, exceptionInfo)
            {
                Response = response
            };

            try
            {
                await this.OnCommandExecutedAsync(executedContext, cancellationToken);
            }
            catch
            {
                // Catch is running because OnCommandExecuted threw an exception, so we just want to re-throw.
                // We also need to reset the response to forget about it since a filter threw an exception.
                handlerContext.Response = null;
                throw;
            }

            if (executedContext.Response != null)
            {
                return executedContext.Response;
            }

            if (executedContext.ExceptionInfo != null)
            {
                if (exceptionInfo == null)
                {
                    executedContext.ExceptionInfo.Throw();
                }
                else
                {
                    Exception newException = executedContext.ExceptionInfo.SourceException;
                    Exception exception = exceptionInfo.SourceException;
                    if (newException == exception)
                    {
                        exceptionInfo.Throw();
                    }
                    else
                    {
                        executedContext.ExceptionInfo.Throw();
                    }
                }
            }

            throw Error.InvalidOperation(Resources.HandlerFilterAttribute_MustSupplyResponseOrException, this.GetType().Name);
        }
    }
}