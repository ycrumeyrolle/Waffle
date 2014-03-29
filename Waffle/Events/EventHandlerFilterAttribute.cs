namespace Waffle.Events
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Tasks;

    /// <summary>
    /// Represents the base class for all handler-filter attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class EventHandlerFilterAttribute : FilterAttribute, IEventHandlerFilter
    {
        /// <summary>
        /// Occurs before the handle method is invoked.
        /// </summary>
        /// <param name="eventOccuringContext">The handler context.</param>
        /// <remarks>
        /// Overrides this method to add a behaviour before an event in a non-asynchronous way.
        /// </remarks>
        public virtual void OnEventOccurring(EventHandlerContext eventOccuringContext)
        {
        }
        
        /// <summary>
        /// Occurs after the handle method is invoked.
        /// </summary>
        /// <param name="eventOccuredContext">The handler executed context.</param>
        /// <remarks>
        /// Overrides this method to add a behaviour after an event in a non-asynchronous way.
        /// </remarks>
        public virtual void OnEventOccurred(EventHandlerOccuredContext eventOccuredContext)
        {
        }

        /// <summary>
        /// Occurs before the handle method is invoked.
        /// </summary>
        /// <param name="eventOccuringContext">The handler context.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <remarks>
        /// Overrides this method to add a behaviour before an event in a asynchronous way.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "exception is flowed through the task")]
        public virtual Task OnEventOccurringAsync(EventHandlerContext eventOccuringContext, CancellationToken cancellationToken)
        {
            try
            {
                this.OnEventOccurring(eventOccuringContext);
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
        /// <param name="eventOccuredContext">The handler context.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <remarks>
        /// Overrides this method to add a behaviour after an event in a asynchronous way.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "exception is flowed through the task")]
        public virtual Task OnEventOccurredAsync(EventHandlerOccuredContext eventOccuredContext, CancellationToken cancellationToken)
        {
            try
            {
                this.OnEventOccurred(eventOccuredContext);
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
        public Task ExecuteHandlerFilterAsync(EventHandlerContext handlerContext, CancellationToken cancellationToken, Func<Task> continuation)
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

        private async Task ExecuteHandlerFilterAsyncCore(EventHandlerContext handlerContext, CancellationToken cancellationToken, Func<Task> continuation)
        {
            await this.OnEventOccurringAsync(handlerContext, cancellationToken);

            await this.CallOnHandlerOccurededAsync(handlerContext, cancellationToken, continuation);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to intercept all exceptions")]
        private async Task CallOnHandlerOccurededAsync(EventHandlerContext handlerContext, CancellationToken cancellationToken, Func<Task> continuation)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ExceptionDispatchInfo exceptionInfo = null;
            try
            {
                await continuation();
            }
            catch (Exception e)
            {
                exceptionInfo = ExceptionDispatchInfo.Capture(e);
            }

            EventHandlerOccuredContext occuredContext = new EventHandlerOccuredContext(handlerContext, exceptionInfo);

            await this.OnEventOccurredAsync(occuredContext, cancellationToken);

            if (occuredContext.ExceptionInfo != null)
            {
                if (exceptionInfo == null)
                {
                    occuredContext.ExceptionInfo.Throw();
                }
                else
                {
                    Exception exception = exceptionInfo.SourceException;
                    Exception newException = occuredContext.ExceptionInfo.SourceException;
                    if (newException == exception)
                    {
                        exceptionInfo.Throw();
                    }
                    else
                    {
                        occuredContext.ExceptionInfo.Throw();
                    }
                }
            }
        }
    }
}