namespace Waffle.Events
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an event handler. 
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This interface is similar to the EventHandler from .Net.")]
    [Handler]
    public interface IAsyncEventHandler<in TEvent> : IEventHandler where TEvent : IEvent
    {
        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "event", Justification = "This interface is similar to the EventHandler from .Net.")]
        Task HandleAsync(TEvent @event);
    }
}