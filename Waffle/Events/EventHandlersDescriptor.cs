namespace Waffle.Events
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides information about the list of event handler descriptors.
    /// </summary>
    public class EventHandlersDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandlersDescriptor"/> class.
        /// </summary>
        /// <param name="eventName"></param>
        public EventHandlersDescriptor(string eventName)
            : this(eventName, new List<EventHandlerDescriptor>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandlersDescriptor"/> class.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventHandlerDescriptors"></param>
        public EventHandlersDescriptor(string eventName, IList<EventHandlerDescriptor> eventHandlerDescriptors)
        {
            this.EventName = eventName;
            this.EventHandlerDescriptors = eventHandlerDescriptors;
        }

        /// <summary>
        /// Gets the name of the event.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; private set; }

        /// <summary>
        /// Gets the collection of <see cref="EventHandlerDescriptor"/>.
        /// </summary>
        /// <value>The collection of <see cref="EventHandlerDescriptor"/>.</value>
        public IList<EventHandlerDescriptor> EventHandlerDescriptors { get; private set; }
    }
}