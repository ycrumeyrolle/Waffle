namespace Waffle.Events.RavenDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Raven.Client;
    using Raven.Client.Document;

    public class RavenEventStore : IEventStore
    {
        private readonly DocumentStore documentStore;

        public RavenEventStore(string connectionStringName)
        {
            this.documentStore = new DocumentStore
            {
                Url = connectionStringName
            };
            this.documentStore.Initialize();
        }

        public async Task StoreAsync(IEvent @event, string eventName, CancellationToken cancellationToken)
        {
            using (IAsyncDocumentSession session = this.documentStore.OpenAsyncSession())
            {
                Event eventEntity = new Event(eventName, @event);
                await session.StoreAsync(eventEntity);
                await session.SaveChangesAsync();
            }
        }

        public async Task<ICollection<IEvent>> LoadAsync(Guid sourceId, CancellationToken cancellationToken)
        {
            using (IAsyncDocumentSession session = this.documentStore.OpenAsyncSession())
            {
                var events = await session.Query<Event>()
                    .Where(e => e.SourceId == sourceId)
                    .Select(e => e.Payload)
                    .ToListAsync();
                return events;
            }
        }
        
        private class Event
        {
            public Event(string eventName, IEvent payload)
            {
                if (payload == null)
                {
                    throw new ArgumentNullException("payload");
                }

               // this.Id = Guid.NewGuid();
                this.CreationDate = DateTime.Now;
                this.SourceId = payload.SourceId;
                this.EventName = eventName;
                this.Payload = payload;
            }

            public string Id { get; set; }

            public Guid SourceId { get; set; }

            public string EventName { get; set; }

            public DateTime CreationDate { get; set; }

          //  [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
            public IEvent Payload { get; set; }
        }
    }
}
