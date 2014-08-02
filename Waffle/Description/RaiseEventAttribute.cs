namespace Waffle.Description
{
    using System;

    /// <summary>
    /// Use this to specify the event type raised by an handler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class RaiseEventAttribute : Attribute
    {
        private readonly Type eventType;

        /// <summary>
        /// Initializes a new instance of the <see cref="RaiseEventAttribute"/> class.
        /// </summary> 
        /// <param name="eventType">The event type.</param>
        public RaiseEventAttribute(Type eventType)
        {
            this.eventType = eventType;
        }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public Type EventType
        {
            get { return this.eventType; }
        }

        /// <summary>
        /// Gets or sets the condition that raise the event.
        /// </summary>
        public string Condition { get; set; }
    }
}
