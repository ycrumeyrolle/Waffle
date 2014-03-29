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
        
        public RaiseEventAttribute(Type eventType)
        {
            this.eventType = eventType;
        }

        public Type EventType
        {
            get { return this.eventType; }
        } 
    }
}
