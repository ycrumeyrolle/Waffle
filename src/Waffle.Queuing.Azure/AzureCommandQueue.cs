namespace Waffle.Queuing.Azure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.WindowsAzure;
    using Newtonsoft.Json;
    using Waffle.Commands;

    public sealed class AzureCommandQueue : ICommandSender, ICommandReceiver, IDisposable
    {
        private const string QueueName = "WaffleQueue";

        private readonly IQueueClient client;

        private bool disposed;
 
        private static readonly JsonSerializerSettings SerializationSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCommandQueue"/> class.
        /// </summary>
        public AzureCommandQueue(string connectionString)
        {
            this.client = new DefaultQueueClient(QueueClient.CreateFromConnectionString(connectionString, QueueName));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCommandQueue"/> class.
        /// </summary>
        public AzureCommandQueue(IQueueClient client)
        {
            this.client = client;
        }

        /// <inheritsdoc />
        public bool IsCompleted
        {
            get { return this.client.IsClosed; }
        }

        /// <inheritsdoc />
        public void Complete()
        {
            this.client.Close();
        }

        /// <inheritsdoc />
        public Task SendAsync(ICommand command, CancellationToken cancellationToken)
        {
            string value = JsonConvert.SerializeObject(command, SerializationSettings);
            return this.client.SendAsync(new BrokeredMessage(value));
        }

        /// <inheritsdoc />
        public async Task<ICommand> ReceiveAsync(CancellationToken cancellationToken)
        {
            var message = await this.client.ReceiveAsync();
            string body = message.GetBody<string>();
            var command = JsonConvert.DeserializeObject<ICommand>(body, SerializationSettings);
            return command;
        }

        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Complete();
            this.disposed = true;
        }
    }
}
