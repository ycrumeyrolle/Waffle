namespace Waffle.Queuing.Azure.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Moq;
    using Newtonsoft.Json;
    using Ploeh.SemanticComparison;
    using Waffle.Commands;
    using Xunit;
    using Xunit.Sdk;

    public class AzureCommandQueueTests
    {
        [Fact]
        public async Task ReceiveAsync()
        {
            JsonSerializerSettings serializationSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var command = new FakeCommand("test");
            var client = new Mock<IQueueClient>();
            client
                .Setup(c => c.ReceiveAsync())
                .Returns(Task.FromResult(new BrokeredMessage(JsonConvert.SerializeObject(command, serializationSettings))));

            using (AzureCommandQueue queue = new AzureCommandQueue(client.Object))
            {
                ICommand result = await queue.ReceiveAsync(CancellationToken.None);

                Assert.NotNull(result);
                AssertLikeness.Like(command, result);
            }
        }

        [Fact]
        public async Task SendAsync()
        {
            var command = new FakeCommand("test");
            var client = new Mock<IQueueClient>();

            using (AzureCommandQueue queue = new AzureCommandQueue(client.Object))
            {
                await queue.SendAsync(command, CancellationToken.None);

                client.Verify(c => c.SendAsync(It.IsAny<BrokeredMessage>()), Times.Once());
            }
        }

        [Fact]
        public void Disposed_ThenIsClosed()
        {
            var client = new Mock<IQueueClient>();
            AzureCommandQueue queue = new AzureCommandQueue(client.Object);
            queue.Dispose();

            client.Verify(c => c.Close(), Times.Once());
        }

        [Fact]
        public void Completed_ThenIsClosed()
        {
            var client = new Mock<IQueueClient>();
            AzureCommandQueue queue = new AzureCommandQueue(client.Object);
            queue.Complete();

            client.Verify(c => c.Close(), Times.Once());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsCompleted(bool closed)
        {
            var client = new Mock<IQueueClient>();
            client
                .Setup(c => c.IsClosed)
                .Returns(closed);
            AzureCommandQueue queue = new AzureCommandQueue(client.Object);
            queue.Complete();

            Assert.Equal(closed, queue.IsCompleted);
        }

        private class FakeCommand : ICommand
        {
            public FakeCommand(string value)
            {
                this.Value = value;
            }

            public string Value { get; set; }
        }
    }

    static class AssertLikeness
    {
        public static void Like<T>(T expected, T actual)
        {
            Likeness<T> likeness = new Likeness<T>(expected);
            if (!likeness.Equals(actual))
            {
                throw new EqualException(expected, actual);
            }
        }
    }
}
