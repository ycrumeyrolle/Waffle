namespace Waffle.Filters
{
    using System.Runtime.ExceptionServices;
    using Waffle.Events;
    using Waffle.Internal;

    /// <summary>
    /// Contains information for the executed handler.
    /// </summary>
    public class EventHandlerOccurredContext
    {
        private readonly EventHandlerContext handlerContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandlerOccurredContext"/> class.
        /// </summary>
        /// <param name="handlerContext">Then handler context.</param>
        /// <param name="exceptionInfo">The <see cref="ExceptionDispatchInfo"/>. Optionnal.</param>
        public EventHandlerOccurredContext(EventHandlerContext handlerContext, ExceptionDispatchInfo exceptionInfo)
        {
            if (handlerContext == null)
            {
                throw Error.ArgumentNull("handlerContext");
            }

            this.handlerContext = handlerContext;
            this.ExceptionInfo = exceptionInfo;
        }

        /// <summary>
        /// Gets or sets the handler context. 
        /// </summary>
        /// <value>The handler context.</value>
        public EventHandlerContext HandlerContext
        {
            get
            {
                return this.handlerContext;
            }
        }

        /// <summary>
        /// Gets or sets the exception that was raised during the execution.
        /// </summary>
        /// <value>The exception that was raised during the execution.</value>
        public ExceptionDispatchInfo ExceptionInfo { get; set; }
        
        /// <summary>
        /// Gets the current <see cref="IEvent"/>.
        /// </summary>
        /// <value>
        /// The <see cref="IEvent"/>.
        /// </value>
        public IEvent Event
        {
            get
            {
                return this.handlerContext.Event;
            }
        }

        /// <summary>
        /// Gets the current <see cref="HandlerRequest"/>.
        /// </summary>
        /// <value>
        /// The <see cref="HandlerRequest"/>.
        /// </value>
        public HandlerRequest Request
        {
            get
            {
                return this.handlerContext.Request;
            }
        }
    }
}