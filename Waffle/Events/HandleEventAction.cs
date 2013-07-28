namespace Waffle.Events
{
    public delegate void HandleEventAction(IEventHandler handler, IEvent @event, EventHandlerContext context);
}