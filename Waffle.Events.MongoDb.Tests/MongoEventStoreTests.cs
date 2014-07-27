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
    using Xunit;

    public class MongoEventStoreTests
    {
        [Fact]
        public void LoadAsync_ReturnsResults()
        {
            StubStore eventStore = new StubStore("mongodb://localhost:27017", "eventsTest");
            EventTest1 event1 = new EventTest1 { Test1 = "test", SourceId = Guid.NewGuid() };
            EventTest2 event2 = new EventTest2 { Test2 = "test", SourceId = Guid.NewGuid() };
            EventWrapper[] events = { new EventWrapper(event1), new EventWrapper(event2) };

            eventStore.MoqCollection
                .Setup(c => c.FindAs(It.IsAny<Type>(), It.IsAny<IMongoQuery>()))
                .Returns(CreateCursor(eventStore.MoqCollection.Object, events));

            var task1 = eventStore.LoadAsync(Guid.Empty, default(CancellationToken));
            var result1 = task1.Result;
            Assert.NotNull(result1);
            Assert.Equal(2, result1.Count);
        }

        [Fact]
        public async void StoreAsync_InsertIntoCollection()
        {
            StubStore eventStore = new StubStore("mongodb://localhost:27017", "eventsTest");
            EventTest1 @event1 = new EventTest1 { Test1 = "test", SourceId = Guid.NewGuid() };
            eventStore.MoqCollection
                 .Setup(c => c.Insert(It.IsAny<EventWrapper>()))
                 .Returns(new WriteConcernResult(new BsonDocument()));
            await eventStore.StoreAsync(@event1, default(CancellationToken));

            eventStore.MoqCollection.Verify(c => c.Insert(It.IsAny<EventWrapper>()), Times.Once());
        }

        public class EventTest1 : IEvent
        {
            /// <summary>
            /// Gets the identifier of the source originating the event.
            /// </summary>
            /// <value>The identifier of the source originating the event.</value>
            public Guid SourceId { get; set; }

            public string Test1 { get; set; }
        }

        public class EventTest2 : IEvent
        {
            /// <summary>
            /// Gets the identifier of the source originating the event.
            /// </summary>
            /// <value>The identifier of the source originating the event.</value>
            public Guid SourceId { get; set; }

            public string Test2 { get; set; }
        }

        private class StubStore : MongoEventStore
        {
            public StubStore(string connectionString, string databaseName)
                : base(connectionString, databaseName)
            {
                this.InitializeCollection();
            }

            protected override MongoCollection<EventWrapper> GetCollection()
            {
                return this.MoqCollection.Object;
            }

            public Mock<MongoCollection<EventWrapper>> MoqCollection { get; private set; }

            private void InitializeCollection()
            {
                var server = CreateServer();
                var database = CreateDatabase(server);

                this.MoqCollection = CreateCollection<EventWrapper>(database.Object);
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
            var collection = new Mock<MongoCollection<T>>(MockBehavior.Strict, database, typeof(T).Name, collectionSettings);
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