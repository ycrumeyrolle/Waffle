namespace Waffle.Tests.Queueing
{
    using Moq;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Queuing;
    using Xunit;

    public class CommandBrokerFixture
    {
        [Fact]
        public async Task RunAsync_RequestsAreConsumed()
        {
            // Arrange
            const int CommandCount = 150000;
            var collection = new ConcurrentQueue<ICommand>();
            for (int i = 0; i < CommandCount; i++)
            {
                collection.Enqueue(new CommandToQueue(i));
            }

            InMemoryCommandQueue queue = new InMemoryCommandQueue(collection);
            
            Mock<IMessageProcessor> processor = new Mock<IMessageProcessor>(MockBehavior.Strict);
            CancellationTokenSource cancellation = new CancellationTokenSource();
            processor
                .Setup(p => p.ProcessAsync(It.IsAny<CommandToQueue>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new HandlerResponse()));

            CommandBroker broker = new CommandBroker(processor.Object, queue, 8);
            broker.Complete();

            // Act
            await broker.RunAsync(cancellation.Token);

            // Assert
            processor.Verify(p => p.ProcessAsync(It.IsAny<CommandToQueue>(), It.IsAny<CancellationToken>()), Times.Exactly(CommandCount));
        }

        private class CommandToQueue : ICommand
        {
            public CommandToQueue(int number)
            {
                this.Number = number;
            }

            public int Number { get; set; }
        }
    }
}
