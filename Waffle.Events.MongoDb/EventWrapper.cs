namespace Waffle.Events.MongoDb
{
    using System;

    /// <summary>
    /// Represents a wrapper to store an event in MongoDB.
    /// </summary>
    public class EventWrapper
    {
        public EventWrapper(string eventName, IEvent payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            this.Id = Guid.NewGuid();
            this.CreationDate = DateTime.Now;
            this.SourceId = payload.SourceId;
            this.EventName = eventName;
            this.Payload = payload;
        }

        public Guid Id { get; set; }

        public Guid SourceId { get; set; }

        public string EventName { get; set; }

        public DateTime CreationDate { get; set; }

        public IEvent Payload { get; set; }
    }
}
