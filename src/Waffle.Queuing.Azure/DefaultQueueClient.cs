namespace Waffle.Queuing.Azure
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    internal class DefaultQueueClient : IQueueClient
    {
        private readonly QueueClient inner;

        public DefaultQueueClient(QueueClient inner)
        {
            this.inner = inner;
        }

        public bool IsClosed { get; set; }

        public void Close()
        {
            this.inner.Close();
        }

        public Task SendAsync(BrokeredMessage message)
        {
            return this.inner.SendAsync(message);
        }

        public Task<BrokeredMessage> ReceiveAsync()
        {
            return this.inner.ReceiveAsync();
        }
    }
}