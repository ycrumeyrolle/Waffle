namespace CommandProcessing.Eventing
{
    using CommandProcessing.Filters;
    using CommandProcessing.Services;

    public class EventAttribute : HandlerFilterAttribute
    {
        public EventAttribute(string eventName)
        {
            this.EventName = eventName;
        }

        public string EventName { get; private set; }
        
        public override void OnCommandExecuted(HandlerExecutedContext context)
        {
            var hub = context.HandlerContext.Processor.Configuration.Services.GetMessageHub();
            hub.Publish(this.EventName, context.Command);
        }
    }
}
