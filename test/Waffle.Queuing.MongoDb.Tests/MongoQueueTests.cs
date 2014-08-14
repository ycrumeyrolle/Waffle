namespace Waffle.Queuing.MongoDb.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Driver;
    using Moq;
    using Waffle.Commands;
    using Waffle.Queuing.MongoDb;
    using Xunit;

    public class MongoQueueTests
    {
        [Fact]
        public async Task SendAsync_RequestInQueue()
        {
            // Arrange
            StubQueue queue = new StubQueue("mongodb://localhost:27017", "eventsTest", "testQueue");
            var command = new CommandToQueue(1, "test", DateTime.Now.AddMinutes(10));
            List<CommandWrapper> repository = new List<CommandWrapper>();
            queue.MoqCollection
                 .Setup(c => c.Insert(It.IsAny<CommandWrapper>()))
                 .Returns(new WriteConcernResult(new BsonDocument()))
                 .Callback<CommandWrapper>(repository.Add);
           
            // Act
            await queue.SendAsync(command, default(CancellationToken));
          
            // Assert
            var result = repository.FirstOrDefault();
            Assert.NotNull(result);
            Assert.IsType(typeof(CommandToQueue), result.Payload);
            var resultCommand = (CommandToQueue)result.Payload;
            Assert.Equal(command.IntValue, resultCommand.IntValue);
            Assert.Equal(command.StringValue, resultCommand.StringValue);
            Assert.Equal(command.DateTimeValue, resultCommand.DateTimeValue);
        }

        [Fact]
        public async Task ReceiveAsync_EmptyQueue_ReturnsNull()
        {
            // Arrange
            StubQueue queue = new StubQueue("mongodb://localhost:27017", "eventsTest", "testQueue");
            queue.MoqCollection
                .Setup(c => c.FindAs<CommandWrapper>(It.IsAny<IMongoQuery>()))
                .Returns(CreateCursor(queue.MoqCollection.Object, new CommandWrapper[0]));

            // Act
            var result = await queue.ReceiveAsync(default(CancellationToken));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ReceiveAsync_EmptyQueue_ReturnsCommand()
        {
            // Arrange
            StubQueue queue = new StubQueue("mongodb://localhost:27017", "eventsTest", "testQueue");
            var command = new CommandToQueue(1, "test", DateTime.Now.AddMinutes(10));
            queue.MoqCollection
                .Setup(c => c.FindAs<CommandWrapper>(It.IsAny<IMongoQuery>()))
                .Returns(CreateCursor(queue.MoqCollection.Object, new[] { new CommandWrapper(command) }));

            // Act
            var result = await queue.ReceiveAsync(default(CancellationToken));

            // Assert
            Assert.NotNull(result);
            Assert.IsType(typeof(CommandToQueue), result);
            var resultCommand = (CommandToQueue)result;
            Assert.Equal(command.IntValue, resultCommand.IntValue);
            Assert.Equal(command.StringValue, resultCommand.StringValue);
            Assert.Equal(command.DateTimeValue, resultCommand.DateTimeValue);

        }
        
        private class CommandToQueue : ICommand
        {
            public CommandToQueue(int intValue, string stringValue, DateTime dateTimeValue)
            {
                this.IntValue = intValue;
                this.StringValue = stringValue;
                this.DateTimeValue = dateTimeValue;
            }

            public int IntValue { get; set; }

            public string StringValue { get; set; }

            public DateTime DateTimeValue { get; set; }
        }

        private class StubQueue : MongoCommandQueue
        {
            public StubQueue(string connectionString, string databaseName, string collectionName)
                : base(connectionString, databaseName, collectionName)
            {
                this.InitializeCollection();
            }

            protected override MongoCollection<CommandWrapper> GetCollection()
            {
                if (this.MoqCollection == null)
                {
                    this.InitializeCollection();
                }
                
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

            var database = new Mock<MongoDatabase>(MockBehavior.Strict, server.Object, "test", databaseSettings);
            database.Setup(db => db.Settings).Returns(databaseSettings);
            database.Setup(db => db.IsCollectionNameValid(It.IsAny<string>(), out message)).Returns(true);
            database.Setup(db => db.CollectionExists(It.IsAny<string>())).Returns(true);
            database.Setup(db => db.Server).Returns(server.Object);
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

            var server = new Mock<MongoServer>(MockBehavior.Strict, serverSettings);
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
            var cursor = new Mock<MongoCursor<T>>(MockBehavior.Strict, collection, new Mock<IMongoQuery>().Object, ReadPreference.Primary, new Mock<IBsonSerializer>().Object, new Mock<IBsonSerializationOptions>().Object);
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
            cursor.Setup(x => x.SetFlags(It.IsAny<QueryFlags>())).Returns(cursor.Object);
            return cursor.Object;
        }
    }
}
