namespace Waffle.Events
{
    using System;

    /// <summary>
    /// Represents an event message.
    /// </summary>
    public interface IEvent
    {
    }

    public interface ISourceable
    {
        /// <summary>
        /// Gets the identifier of the source originating the event.
        /// </summary>
        /// <value>The identifier of the source originating the event.</value>
        Guid SourceId { get; }
    }
}