using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Waffle.Commands;
using Waffle.Queuing;
using Xunit;

namespace Waffle.Tests.Queueing
{
    public class InMemoryCommandQueueFixture
    {
        [Fact]
        public async Task SendAsync_RequestInQueue()
        {
            // Arrange
            var collection = new ConcurrentQueue<ICommand>();
            InMemoryCommandQueue queue = new InMemoryCommandQueue(collection);
            var command = new CommandToQueue();


            // Act
            await queue.SendAsync(command, default(CancellationToken));

            // Assert
            Assert.Equal(1, collection.Count);
        }

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

        private class CommandToQueue : ICommand
        {
        }
    }
}
