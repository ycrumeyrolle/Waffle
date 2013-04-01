////namespace CommandProcessing.Eventing
////{
////    using System;
////    using CommandProcessing.Filters;
////    using CommandProcessing.Services;

////    public class EventAttribute : FilterAttribute, IHandlerFilter
////    {
////        public EventAttribute(string eventName)
////        {
////            this.EventName = eventName;
////        }

////        public string EventName { get; set; }

////        public void OnCommandExecuting(HandlerExecutingContext context)
////        {
////            var hub = (IMessageHub)context.CommandContext.Processor.Configuration.Services.GetService(typeof(IMessageHub));
////            hub.Send(messageType: "before", eventName: this.EventName, 
////        }

////        public void OnCommandExecuted(HandlerExecutedContext context)
////        {
////            throw new NotImplementedException();
////        }
////    }

////    public interface IMessageHub
////    {
////        void Subscribe(
////    }
////}
