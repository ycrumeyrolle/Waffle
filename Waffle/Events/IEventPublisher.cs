namespace Waffle.Events
{
    /// <summary>
    /// Represents an event publisher. 
    /// </summary>
    public interface IEventPublisher
    {
        IEventProcessor EventProcessor { get; }
    }  
}
