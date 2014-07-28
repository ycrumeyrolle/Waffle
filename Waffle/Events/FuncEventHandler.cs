namespace Waffle.Events
{
    using System;
    using System.Threading.Tasks;

    internal class FuncEventHandler<TEvent> : IAsyncEventHandler<TEvent> where TEvent : IEvent
    {
        public EventHandlerContext EventContext { get; set; }

        public Task HandleAsync(TEvent @event)
        {
            // The method will never be called
            throw new NotImplementedException();
        }
    }
}