namespace Waffle.Events
{
    using System;
    using System.Diagnostics.CodeAnalysis;
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
        /// Occurs after the handle method is invoked.
        /// </summary>
        /// <param name="handlerContext">The handler context.</param>
        public virtual void OnEventOccurred(EventHandlerContext handlerContext)
        {
        }

        /// <summary>
        /// Executes the filter handler asynchronously.
        /// </summary>
        /// <param name="handlerContext">The handler context.</param>
        /// <param name="cancellationToken">The cancellation token assigned for this task.</param>
        /// <param name="continuation">The delegate function to continue after the handler method is invoked.</param>
        /// <returns>The newly created task for this operation.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to intercept all exceptions")]
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

            Task task = continuation();
            return task.Then(
                () =>
                {
                    try
                    {
                        this.OnEventOccurred(handlerContext);
                        return TaskHelpers.Completed();
                    }
                    catch (Exception exception)
                    {
                        return TaskHelpers.FromError(exception);
                    }
                },
                cancellationToken);
        }
    }
}