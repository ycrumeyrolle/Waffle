namespace Waffle.Events.MongoDb
{
    using System;

    /// <summary>
    /// Represents a wrapper to store an event in MongoDB.
    /// </summary>
    public class EventWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventWrapper"/> class.
        /// </summary>
        /// <param name="payload">The payload to wrap.</param>
        public EventWrapper(IEvent payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            this.Id = Guid.NewGuid();
            this.CreationDate = DateTime.Now;
            this.SourceId = payload.SourceId;
            this.Payload = payload;
        }

        /// <summary>
        /// Gets or sets the event id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the source id.
        /// </summary>
        public Guid SourceId { get; set; }
        
        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        public IEvent Payload { get; set; }
    }
}
