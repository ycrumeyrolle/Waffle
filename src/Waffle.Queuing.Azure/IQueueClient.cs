namespace Waffle.Queuing.Azure
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public interface IQueueClient
    {
        bool IsClosed { get; set; }

        void Close();

        Task SendAsync(BrokeredMessage message);

        Task<BrokeredMessage> ReceiveAsync();
    }
}
