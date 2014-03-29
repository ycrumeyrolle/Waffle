namespace Waffle.Events
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents an event handler. 
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This interface is similar to the EventHandler from .Net.")]
    public interface IEventHandler
    {
        EventHandlerContext EventContext { get; set; }
    }

    /// <summary>
    /// Represents an event handler. 
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This interface is similar to the EventHandler from .Net.")]
    [Handler]
    public interface IEventHandler<in TEvent> : IEventHandler where TEvent : IEvent
    {
        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "event", Justification = "This interface is similar to the EventHandler from .Net.")]
        void Handle(TEvent @event);
    }
}
