namespace Waffle.Events
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class EventHandlersDescriptor
    {
        public EventHandlersDescriptor(string eventName, IList<EventHandlerDescriptor> eventHandlerDescriptors)
        {
            this.EventName = eventName;
            this.EventHandlerDescriptors = new ReadOnlyCollection<EventHandlerDescriptor>(eventHandlerDescriptors);
        }

        public string EventName { get; private set; }

        public ICollection<EventHandlerDescriptor> EventHandlerDescriptors { get; private set; }
    }
}