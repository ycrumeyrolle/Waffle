namespace Waffle.Events
{
    using System;

    /// <summary>
    /// Represents an event message.
    /// </summary>
    public interface IEvent
    {
    }

    /// <summary>
    /// Represents a sourceable event message. 
    /// </summary>
    public interface ISourceableEvent
    {
        /// <summary>
        /// Gets the identifier of the source originating the event.
        /// </summary>
        /// <value>The identifier of the source originating the event.</value>
        Guid SourceId { get; }
    }
}