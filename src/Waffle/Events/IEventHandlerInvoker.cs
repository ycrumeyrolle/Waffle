namespace Waffle.Events
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an invoker of <see cref="IEventHandler"/>.
    /// </summary>
    public interface IEventHandlerInvoker
    {
        /// <summary>
        /// Invokes the event handler. 
        /// </summary>
        /// <param name="context">The context of the event handler.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A completion task.</returns>
        Task InvokeHandlerAsync(EventHandlerContext context, CancellationToken cancellationToken);
    }
}