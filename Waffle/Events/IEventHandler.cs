namespace Waffle.Events
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents an event handler. 
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This interface is similar to the EventHandler from .Net.")]
    public interface IEventHandler
    {
        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        /// <param name="context">The <see cref="EventHandlerContext"/>.</param>
        void Handle(IEvent @event, EventHandlerContext context);
    } 

    /// <summary>
    /// Represents an event handler. 
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This interface is similar to the EventHandler from .Net.")]
    public interface IEventHandler<in TEvent> : IEventHandler where TEvent : IEvent
    {
        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        /// <param name="context">The <see cref="EventHandlerContext"/>.</param>
        void Handle(TEvent @event, EventHandlerContext context);
    }  
}
