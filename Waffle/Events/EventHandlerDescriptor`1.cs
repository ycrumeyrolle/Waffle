namespace Waffle.Events
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    public class EventHandlerDescriptor<TEvent> : EventHandlerDescriptor where TEvent : IEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandlerDescriptor"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/>.</param>
        /// <param name="commandType">The type of the event.</param>
        /// <param name="func">The <see cref="Func{TResult}"/> representing the handler.</param>
        public EventHandlerDescriptor(ProcessorConfiguration configuration, Type commandType, Func<TEvent, Task> func)
            : base(configuration, commandType, typeof(FuncEventHandler<TEvent>), func.GetMethodInfo())
        {
        }
    }
}
