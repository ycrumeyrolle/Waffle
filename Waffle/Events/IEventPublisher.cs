namespace Waffle.Events
{
    /// <summary>
    /// Represents an event publisher. 
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        ///  Gets the event processor.
        /// </summary>
        /// <value>The event processor.</value>
        IEventProcessor EventProcessor { get; }
    }  
}
