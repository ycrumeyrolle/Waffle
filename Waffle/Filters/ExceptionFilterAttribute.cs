namespace Waffle.Filters
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Internal;
    using Waffle.Tasks;

    /// <summary>
    /// Represents an attribute that is used to handle an exception that is thrown by an handler method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class ExceptionFilterAttribute : FilterAttribute, IExceptionFilter
    {
        /// <summary>
        /// Raises the exception event.
        /// </summary>
        /// <param name="handlerExecutedContext">The context for the handler..</param>
        public virtual void OnException(HandlerExecutedContext handlerExecutedContext)
        {
        }

        /// <summary>
        /// Asynchronously executes the exception filter.
        /// </summary>
        /// <param name="handlerExecutedContext">The handler executed context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous exception filter.</returns>
        Task IExceptionFilter.ExecuteExceptionFilterAsync(HandlerExecutedContext handlerExecutedContext, CancellationToken cancellationToken)
        {
            if (handlerExecutedContext == null)
            {
                throw Error.ArgumentNull("handlerExecutedContext");
            }

            this.OnException(handlerExecutedContext);
            return TaskHelpers.Completed();
        }
    }
}
