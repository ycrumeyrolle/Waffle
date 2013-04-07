namespace CommandProcessing.Eventing
{
    using System;
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;
    using CommandProcessing.Services;

    /// <summary>
    /// Represents an <see cref="HandlerFilterAttribute"/> to trigger event on command handling.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class EventAttribute : HandlerFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventAttribute"/> class.
        /// </summary>
        /// <param name="eventName">The name of the event to trigger.</param>
        public EventAttribute(string eventName)
        {
            this.EventName = eventName;
        }

        /// <summary>
        /// Gets the name of the event to trigger.
        /// </summary>
        /// <value>The name of the event to trigger.</value>
        public string EventName { get; private set; }

        /// <summary>
        /// Occurs after the handle method is invoked.
        /// </summary>
        /// <param name="handlerExecutedContext">The handler executed context.</param>
        public override void OnCommandExecuted(HandlerExecutedContext handlerExecutedContext)
        {
            if (handlerExecutedContext == null)
            {
                throw Error.ArgumentNull("handlerExecutedContext");
            }

            IMessageHub hub = handlerExecutedContext.HandlerContext.Configuration.Services.GetMessageHub();
            hub.Publish(this.EventName, handlerExecutedContext.Command);
        }
    }
}
