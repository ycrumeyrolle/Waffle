namespace Waffle.Events.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;
    using Waffle.Tasks;

    public class MongoEventStore : IEventStore
    {
        private readonly string databaseName;

        private readonly Func<MongoClient> clientFactory;

        public MongoEventStore(string connectionString, string databaseName)
        {
            this.clientFactory = () => new MongoClient(connectionString);
            this.databaseName = databaseName;
        }

        public MongoEventStore(MongoClientSettings clientSettings, string databaseName)
        {
            this.clientFactory = () => new MongoClient(clientSettings);
            this.databaseName = databaseName;
        }

        public MongoEventStore(MongoUrl url, string databaseName)
            : this(MongoClientSettings.FromUrl(url), databaseName)
        {
        }

        public Task StoreAsync(IEvent @event, string eventName, CancellationToken cancellationToken)
        {
            try
            {
                MongoCollection<EventWrapper> collection = this.GetCollection();
                collection.Insert(new EventWrapper(eventName, @event));

                return TaskHelpers.Completed();
            }
            catch (Exception exception)
            {
                return TaskHelpers.FromError<object>(exception);
            }
        }

        public Task<ICollection<IEvent>> LoadAsync(Guid sourceId, CancellationToken cancellationToken)
        {
            try
            {
                MongoCollection<EventWrapper> collection = this.GetCollection();

                IMongoQuery query = Query.EQ("SourceId", sourceId);
                MongoCursor<EventWrapper> cursor = collection.Find(query);
                ReadOnlyCollection<IEvent> result = new ReadOnlyCollection<IEvent>(cursor.Select(e => e.Payload).ToList());
                return TaskHelpers.FromResult<ICollection<IEvent>>(result);
            }
            catch (Exception exception)
            {
                return TaskHelpers.FromError<ICollection<IEvent>>(exception);
            }
        }

        protected virtual MongoCollection<EventWrapper> GetCollection()
        {
            MongoClient client = this.clientFactory();
            MongoServer server = client.GetServer();
            MongoDatabase database = server.GetDatabase(this.databaseName);

            MongoCollection<EventWrapper> collection = database.GetCollection<EventWrapper>("events");
            return collection;
        }
    }
}
