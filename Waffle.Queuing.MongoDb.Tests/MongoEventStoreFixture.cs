namespace Waffle.Events.MongoDb.Tests
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Driver;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Queuing.MongoDb;
    using Xunit;
        
    public class MongoEventStoreFixture
    {
        [Fact]
        public async Task LoadAsync_ReturnsResults()
        {
            StubStore eventStore = new StubStore("mongodb://localhost:27017", "eventsTest", "testQueue");
            CommandTest1 command1 = new CommandTest1 { Test1 = "test" };
            CommandTest2 command2 = new CommandTest2 { Test2 = "test" };
            CommandWrapper[] commands = { new CommandWrapper(command1), new CommandWrapper(command2) };

            eventStore.MoqCollection
                .Setup(c => c.Find(It.IsAny<IMongoQuery>()))
                .Returns(CreateCursor(eventStore.MoqCollection.Object, commands));

            var result = await eventStore.ReceiveAsync(default(CancellationToken));
            Assert.NotNull(result);
        }

        [Fact]
        public async void StoreAsync_InsertIntoCollection()
        {
            StubStore eventStore = new StubStore("mongodb://localhost:27017", "eventsTest", "testQueue");
            CommandTest1 command1 = new CommandTest1 { Test1 = "test" };

            await eventStore.SendAsync(command1, default(CancellationToken));

            eventStore.MoqCollection.Verify(c => c.Insert(It.IsAny<CommandWrapper>()), Times.Once());
        }

        public class CommandTest1 : ICommand
        {
            public string Test1 { get; set; }
        }

        public class CommandTest2 : ICommand
        {
            public string Test2 { get; set; }
        }

        private class StubStore : MongoQueue
        {
            public StubStore(string connectionString, string databaseName, string collectionName)
                : base(connectionString, databaseName, collectionName)
            {
                this.InitializeCollection();
            }
            
            protected override MongoCollection<CommandWrapper> GetCollection()
            {
                return this.MoqCollection.Object;
            }

            public Mock<MongoCollection<CommandWrapper>> MoqCollection { get; private set; }

            private void InitializeCollection()
            {
                var server = CreateServer();
                var database = CreateDatabase(server);

                this.MoqCollection = CreateCollection<CommandWrapper>(database.Object);
            }
        }

        private static Mock<MongoDatabase> CreateDatabase(Mock<MongoServer> server)
        {
            string message;
            var databaseSettings = new MongoDatabaseSettings
            {
                GuidRepresentation = GuidRepresentation.Standard,
                ReadEncoding = new UTF8Encoding(),
                ReadPreference = new ReadPreference(),
                WriteConcern = new WriteConcern(),
                WriteEncoding = new UTF8Encoding()
            };

            var database = new Mock<MongoDatabase>(server.Object, "test", databaseSettings);
            database.Setup(db => db.Settings).Returns(databaseSettings);
            database.Setup(db => db.IsCollectionNameValid(It.IsAny<string>(), out message)).Returns(true);
            return database;
        }

        private static Mock<MongoServer> CreateServer()
        {
            string message;
            var serverSettings = new MongoServerSettings
            {
                GuidRepresentation = GuidRepresentation.Standard,
                ReadEncoding = new UTF8Encoding(),
                ReadPreference = new ReadPreference(),
                WriteConcern = new WriteConcern(),
                WriteEncoding = new UTF8Encoding()
            };

            var server = new Mock<MongoServer>(serverSettings);
            server.Setup(s => s.Settings).Returns(serverSettings);
            server.Setup(s => s.IsDatabaseNameValid(It.IsAny<string>(), out message)).Returns(true);
            return server;
        }

        private static Mock<MongoCollection<T>> CreateCollection<T>(MongoDatabase database)
        {
            var collectionSettings = new MongoCollectionSettings();
            var collection = new Mock<MongoCollection<T>>(database, typeof(T).Name, collectionSettings);
            collection.SetupAllProperties();
            collection.SetupGet(x => x.Database).Returns(database);
            collection.SetupGet(x => x.Settings).Returns(collectionSettings);
            return collection;
        }

        private static MongoCursor<T> CreateCursor<T>(MongoCollection<T> collection, IEnumerable<T> obj)
        {
            var cursor = new Mock<MongoCursor<T>>(collection, new Mock<IMongoQuery>().Object, ReadPreference.Primary, new Mock<IBsonSerializer>().Object, new Mock<IBsonSerializationOptions>().Object);
            cursor.Setup(x => x.GetEnumerator()).Callback(() => obj.GetEnumerator().Reset()).Returns(obj.GetEnumerator);
            cursor.Setup(x => x.SetSortOrder(It.IsAny<IMongoSortBy>())).Returns(cursor.Object);
            cursor.Setup(x => x.SetSortOrder(It.IsAny<string[]>())).Returns(cursor.Object);
            cursor.Setup(x => x.SetLimit(It.IsAny<int>())).Returns(cursor.Object);
            cursor.Setup(x => x.SetFields(It.IsAny<IMongoFields>())).Returns(cursor.Object);
            cursor.Setup(x => x.SetFields(It.IsAny<string[]>())).Returns(cursor.Object);
            cursor.Setup(x => x.SetFields(It.IsAny<string>())).Returns(cursor.Object);
            cursor.Setup(x => x.SetSkip(It.IsAny<int>())).Returns(cursor.Object);
            cursor.Setup(x => x.SetHint(It.IsAny<string>())).Returns(cursor.Object);
            cursor.Setup(x => x.SetHint(It.IsAny<BsonDocument>())).Returns(cursor.Object);
            cursor.Setup(x => x.SetMax(It.IsAny<BsonDocument>())).Returns(cursor.Object);
            cursor.Setup(x => x.SetMaxScan(It.IsAny<int>())).Returns(cursor.Object);
            cursor.Setup(x => x.SetOption(It.IsAny<string>(), It.IsAny<BsonValue>())).Returns(cursor.Object);
            cursor.Setup(x => x.SetOptions(It.IsAny<BsonDocument>())).Returns(cursor.Object);
            return cursor.Object;
        }
    }
}