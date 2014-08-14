namespace Waffle.Queuing.Redis
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using StackExchange.Redis;
    using Waffle.Commands;

    public sealed class RedisCommandQueue : ICommandSender, ICommandReceiver, IDisposable
    {
        private const string ChannelKey = "WaffleChannel";
        private const string ListKey = "WaffleQueue";
        private const int EmptyQueueInterval = 10000;

        private static readonly JsonSerializerSettings SerializationSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };

        private readonly BlockingCollection<ICommand> queue;
        private readonly ConnectionMultiplexer multiplexer;
        private readonly IDatabase database;
        private readonly ISubscriber pubsub;
        private readonly Timer timer;

        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCommandQueue"/> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        /// <param name="collection">The collection to use as the underlying data store.</param>
        public RedisCommandQueue(string configuration, IProducerConsumerCollection<ICommand> collection)
        {
            this.queue = new BlockingCollection<ICommand>(collection);
            this.multiplexer = ConnectionMultiplexer.Connect(configuration);
            this.database = this.multiplexer.GetDatabase();
            this.pubsub = this.multiplexer.GetSubscriber();
            this.pubsub.Subscribe(ChannelKey, this.HandleMessage);

            this.timer = new Timer(this.FlushQueue, null, EmptyQueueInterval, EmptyQueueInterval);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCommandQueue"/> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        public RedisCommandQueue(string configuration)
            : this(configuration, new ConcurrentQueue<ICommand>())
        {
        }

        /// <inheritsdoc />
        public bool IsCompleted
        {
            get { return this.queue.IsCompleted; }
        }

        /// <summary>
        /// Flush the Redis queue.
        /// </summary>
        /// <param name="state">The state object. Always <c>null</c>.</param>
        private void FlushQueue(object state)
        {
            if (this.disposed)
            {
                return;
            }

            long queueLength = this.database.ListLength(ListKey);
            if (queueLength != 0L)
            {
                for (int i = 0; i < queueLength; i++)
                {
                    if (!this.TryDequeueCommand())
                    {
                        break;
                    }
                }
            }
        }

        private void HandleMessage(RedisChannel channel, RedisValue key)
        {
            this.TryDequeueCommand();
        }

        private bool TryDequeueCommand()
        {
            string result = this.database.ListRightPop(ListKey);
            var command = JsonConvert.DeserializeObject<ICommand>(result, SerializationSettings);
            if (command != null)
            {
                this.queue.Add(command);
                return true;
            }

            return false;
        }

        /// <inheritsdoc />
        public void Complete()
        {
            this.pubsub.UnsubscribeAll(CommandFlags.FireAndForget);
            this.queue.CompleteAdding();
        }

        /// <inheritsdoc />
        public async Task SendAsync(ICommand command, CancellationToken cancellationToken)
        {
            string value = JsonConvert.SerializeObject(command, SerializationSettings);
            await this.database.ListLeftPushAsync(ListKey, value, When.Always, CommandFlags.FireAndForget);
            await this.pubsub.PublishAsync(ChannelKey, ListKey);
        }

        /// <inheritsdoc />
        public Task<ICommand> ReceiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                ICommand command = this.queue.Take(cancellationToken);
                return Task.FromResult(command);
            }
            catch (OperationCanceledException)
            {
                return Task.FromResult<ICommand>(null);
            }
        }

        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.timer.Dispose();
            this.multiplexer.Dispose();
            this.queue.Dispose();
            this.disposed = true;
        }
    }
}
