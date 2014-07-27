using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Waffle.Commands;
using Waffle.Queuing;
using Waffle.Queuing.MongoDb;
using Xunit;

namespace Waffle.Tests.Queueing
{
    public class MongoQueueFixture
    {
        [Fact]
        public async Task SendAsync_RequestInQueue()
        {
            // Arrange
            MongoQueue queue = new MongoQueue("mongodb://localhost:27017", "eventsTest", "testQueue");
            var command = new CommandToQueue(1, "test", DateTime.Now.AddMinutes(10));

            // Act
            await queue.SendAsync(command, default(CancellationToken));
            var result = await queue.ReceiveAsync(default(CancellationToken));

            // Assert
            Assert.NotNull(result);
            Assert.IsType(typeof(CommandToQueue), result);
            var resultCommand = (CommandToQueue)result;
            Assert.Equal(command.IntValue, resultCommand.IntValue);
            Assert.Equal(command.StringValue, resultCommand.StringValue);
            Assert.Equal(command.DateTimeValue, resultCommand.DateTimeValue);
        }
        /*
        [Fact]
        public async Task ReceiveAsync_EmptyQueue_()
        {
            // Arrange
            var collection = new ConcurrentQueue<ICommand>();
            InMemoryCommandQueue queue = new InMemoryCommandQueue(collection);
            collection.Enqueue(new CommandToQueue());
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken cancellationToken = source.Token;
            source.CancelAfter(200);
            ICommand command = null;

            // Act
            try
            {
                command = await queue.ReceiveAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }

            // Assert
            Assert.Null(command);
        }
        */
        private class CommandToQueue : ICommand
        {
            public CommandToQueue ()
	        {

	        }

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
    }
}
