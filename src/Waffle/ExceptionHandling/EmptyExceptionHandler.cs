namespace Waffle.ExceptionHandling
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Internal;
    using Waffle.Tasks;
    
    /// <summary>
    /// Represents an exception handler that leaves exceptions unhandled (allowing them to propagate).
    /// </summary>
    /// <remarks>
    /// This class represents the behavior of having no IExceptionHandler service, such as when the registered service
    /// is removed (Null Object pattern).
    /// </remarks>
    internal class EmptyExceptionHandler : IExceptionHandler
    {
        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            // For exceptions at the top of the call stack, Result will start out non-null (due to
            // LastChanceExceptionHandler). This class does not force exceptions back to unhandled in such cases, so it
            // will not not trigger the host-level exception processing.
            context.Result = null;
            return TaskHelpers.Completed();
        }
    }
}
