namespace CommandProcessing.Eventing
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;
    using CommandProcessing.Services;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute could be inherited.")]
    public class EventAttribute : HandlerFilterAttribute
    {
        public EventAttribute(string eventName)
        {
            this.EventName = eventName;
        }

        public string EventName { get; private set; }

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
