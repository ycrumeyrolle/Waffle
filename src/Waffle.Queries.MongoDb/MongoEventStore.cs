////namespace Waffle.Events.MongoDb
////{
////    using System;
////    using System.Collections.Generic;
////    using System.Collections.ObjectModel;
////    using System.Linq;
////    using System.Threading;
////    using System.Threading.Tasks;
////    using MongoDB.Driver;
////    using MongoDB.Driver.Builders;
////    using Waffle.Tasks;
////    using Waffle.Queries;
////    using MongoDB.Driver.Linq;

////    /// <summary>
////    /// Represents a store with MongoDB.
////    /// </summary>
////    public class MongoEventStore : IQueryContext
////    {
////        private readonly string databaseName;

////        private readonly Func<MongoClient> clientFactory;

////        /// <summary>
////        /// Initializes a new instance of the <see cref="MongoEventStore"/> class.
////        /// </summary>
////        /// <param name="connectionString">The connection string.</param>
////        /// <param name="databaseName">The database name.</param>
////        public MongoEventStore(string connectionString, string databaseName)
////        {
////            this.clientFactory = () => new MongoClient(connectionString);
////            this.databaseName = databaseName;
////        }

////        /// <summary>
////        /// Initializes a new instance of the <see cref="MongoEventStore"/> class.
////        /// </summary>
////        /// <param name="clientSettings">The <see cref="MongoClientSettings"/>.</param>
////        /// <param name="databaseName">The database name.</param>
////        public MongoEventStore(MongoClientSettings clientSettings, string databaseName)
////        {
////            this.clientFactory = () => new MongoClient(clientSettings);
////            this.databaseName = databaseName;
////        }

////        /// <summary>
////        /// Initializes a new instance of the <see cref="MongoEventStore"/> class.
////        /// </summary>
////        /// <param name="url">The MongoURL.</param>
////        /// <param name="databaseName">The database name.</param>
////        public MongoEventStore(MongoUrl url, string databaseName)
////            : this(MongoClientSettings.FromUrl(url), databaseName)
////        {
////        }

////        /// <summary>
////        /// Stores an event.
////        /// </summary>
////        /// <param name="event">The <see cref="IEvent"/> to store.</param>
////        /// <param name="cancellationToken">A cancellation token.</param>
////        /// <returns>A <see cref="Task"/> of the storing.</returns>
////        public Task StoreAsync(IEvent @event, CancellationToken cancellationToken)
////        {
////            try
////            {
////                MongoCollection<EventWrapper> collection = this.GetCollection();
////                collection.Insert(new EventWrapper(@event));

////                return TaskHelpers.Completed();
////            }
////            catch (Exception exception)
////            {
////                return TaskHelpers.FromError<object>(exception);
////            }
////        }

////        /// <summary>
////        /// Load a collection of events.
////        /// </summary>
////        /// <param name="sourceId">The event source identifier.</param>
////        /// <param name="cancellationToken">A cancellation token.</param>
////        /// <returns>A <see cref="Task"/> of <see cref="ICollection{IEvent}"/> containing the <see cref="IEvent"/>.</returns>
////        public Task<ICollection<IEvent>> LoadAsync(Guid sourceId, CancellationToken cancellationToken)
////        {
////            try
////            {
////                MongoCollection<EventWrapper> collection = this.GetCollection();

////                var events = collection.AsQueryable()
////                    .Where(e => e.SourceId == sourceId)
////                    .Select(e => e.Payload)
////                    .ToList();
////                ReadOnlyCollection<IEvent> result = new ReadOnlyCollection<IEvent>(events);
////                return Task.FromResult<ICollection<IEvent>>(result);
////            }
////            catch (Exception exception)
////            {
////                return TaskHelpers.FromError<ICollection<IEvent>>(exception);
////            }
////        }

////        /// <summary>
////        /// Retrives the <see cref="MongoCollection"/>.
////        /// </summary>
////        /// <returns>The <see cref="MongoCollection"/>.</returns>
////        protected virtual MongoCollection<EventWrapper> GetCollection()
////        {
////            MongoClient client = this.clientFactory();
////            MongoServer server = client.GetServer();
////            MongoDatabase database = server.GetDatabase(this.databaseName);

////            MongoCollection<EventWrapper> collection = database.GetCollection<EventWrapper>("events");
////            return collection;
////        }
////    }
////}
