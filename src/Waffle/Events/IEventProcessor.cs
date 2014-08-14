namespace Waffle.Events
{
    /// <summary>
    /// Represents an event processor. 
    /// </summary>
    public interface IEventProcessor
    {
        /// <summary>
        /// Publish the event.
        /// </summary>
        /// <param name="command">The <see cref="IEvent"/> to handle.</param>
        /// <returns>The result object.</returns>
        void Publish(IEvent command);
    }  
}
