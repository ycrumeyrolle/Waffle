namespace Waffle.Queuing.MongoDb
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;
    using Waffle.Commands;

    public class MongoQueue : ICommandSender, ICommandReceiver
    {
        private readonly string databaseName;

        private readonly string collectionName;

        private readonly Func<MongoClient> clientFactory;

        private object collectionInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQueue"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="databaseName">The database name.</param>
        /// <param name="collectionName">The collection name.</param>
        public MongoQueue(string connectionString, string databaseName, string collectionName)
            : this(() => new MongoClient(connectionString), databaseName, collectionName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQueue"/> class.
        /// </summary>
        /// <param name="clientSettings">The <see cref="MongoClientSettings"/>.</param>
        /// <param name="databaseName">The database name.</param>
        /// <param name="collectionName">The collection name.</param>
        public MongoQueue(MongoClientSettings clientSettings, string databaseName, string collectionName)
            : this(() => new MongoClient(clientSettings), databaseName, collectionName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQueue"/> class.
        /// </summary>
        /// <param name="url">The MongoURL.</param>
        /// <param name="databaseName">The database name.</param>
        /// <param name="collectionName">The collection name.</param>
        public MongoQueue(MongoUrl url, string databaseName, string collectionName)
            : this(MongoClientSettings.FromUrl(url), databaseName, collectionName)
        {
        }

        public MongoQueue(Func<MongoClient> clientFactory, string databaseName, string collectionName)
        {
            this.clientFactory = clientFactory;
            this.databaseName = databaseName;
            this.collectionName = collectionName;
        }

        /// <summary>
        /// Retrives the <see cref="MongoCollection"/>.
        /// </summary>
        /// <returns>The <see cref="MongoCollection"/>.</returns>
        protected virtual MongoCollection<CommandWrapper> GetCollection()
        {
            LazyInitializer.EnsureInitialized(ref this.collectionInitialized, this.EnsureCollection);
            MongoDatabase database = GetDatabase();

            MongoCollection<CommandWrapper> collection = database.GetCollection<CommandWrapper>(this.collectionName);
            return collection;
        }

        private object EnsureCollection()
        {
            MongoDatabase database = GetDatabase();

            if (!database.CollectionExists(this.collectionName))
            {
                var options = CollectionOptions
                    .SetCapped(true)
                    .SetMaxSize(5000)
                    .SetMaxDocuments(100);

                database.CreateCollection(this.collectionName, options);
            }

            return new object();
        }

        private MongoDatabase GetDatabase()
        {
            MongoClient client = this.clientFactory();
            MongoServer server = client.GetServer();
            MongoDatabase database = server.GetDatabase(this.databaseName);
            return database;
        }

        public Task SendAsync(ICommand command, CancellationToken cancellationToken)
        {
            var collection = this.GetCollection();
            collection.Insert(new CommandWrapper(command));
            return Task.FromResult(0);
        }

        public Task<ICommand> ReceiveAsync(CancellationToken cancellationToken)
        {
            var collection = this.GetCollection();

            var result = collection
                .FindAs<CommandWrapper>(Query.Null)
                .SetFlags(QueryFlags.AwaitData | QueryFlags.TailableCursor)
                .Select(w => w.Payload)
                .FirstOrDefault();

            return Task.FromResult(result);
        }
        
        public bool IsCompleted
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        public void Complete()
        {
            throw new NotImplementedException();    
        }
    }
}
