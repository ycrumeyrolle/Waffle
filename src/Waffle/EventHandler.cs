namespace Waffle
{
    using Waffle.Events;

    /// <summary>
    /// Represents a base implementation of the event handler and the event handler. 
    /// </summary>
    public abstract class EventHandler : IEventHandler
    {
        /// <summary>
        /// Gets or sets the current <see cref="EventHandlerContext"/>
        /// </summary>
        public EventHandlerContext EventContext { get; set; }
    }
}