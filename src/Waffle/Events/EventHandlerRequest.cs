namespace Waffle.Events
{
    using Waffle.Internal;

    /// <summary>
    /// Represents a request for an handler.    
    /// The <see cref="EventHandlerRequest"/> is responsible to encapsulate 
    /// all informations around a call to an handler.
    /// </summary>
    public sealed class EventHandlerRequest : HandlerRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerRequest"/> class. 
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="event">The event.</param>
        /// <param name="parentRequest">The parent request. </param>
        public EventHandlerRequest(ProcessorConfiguration configuration, IEvent @event, HandlerRequest parentRequest)
            : base(configuration, parentRequest)
        {
            if (@event == null)
            {
                throw Error.ArgumentNull("event");
            }

            // Events cannot be called without a parent request
            if (parentRequest == null)
            {
                throw Error.ArgumentNull("parentRequest");
            }

            this.Event = @event;
            this.MessageType = @event.GetType();
        }
        
        /// <summary>
        /// Gets the current <see cref="IEvent"/>.
        /// </summary>
        /// <value>
        /// The <see cref="IEvent"/>.
        /// </value>
        public IEvent Event { get; private set; }
    }
}