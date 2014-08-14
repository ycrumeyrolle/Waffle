namespace Waffle.Events
{   
    /// <summary>
    /// Represents an action from an event.
    /// </summary>
    /// <param name="handler">The event handler type.</param>
    /// <param name="event">The command.</param>
    /// <param name="context">The handler context.</param>
    public delegate void HandleEventAction(IEventHandler handler, IEvent @event, EventHandlerContext context);
}