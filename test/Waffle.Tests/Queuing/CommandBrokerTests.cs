namespace Waffle.Tests.Queuing
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Waffle.Commands;
    using Waffle.Queuing;
    using Xunit;

    public class CommandBrokerTests
    {
        [Fact]
        public async Task RunAsync_RequestsAreConsumed()
        {
            // Arrange
            const int CommandCount = 100;
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
                .Returns(Task.FromResult(HandlerResponse.Empty));

            CommandRunner broker = new CommandRunner(processor.Object, queue, 8);

            // Act
            cancellation.CancelAfter(1000);
            await broker.StartAsync(cancellation.Token);

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
