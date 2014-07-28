namespace Waffle.Queuing.Redis
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ctstone.Redis;
    using Newtonsoft.Json;
    using Waffle.Commands;

    public class RedisQueue : ICommandSender, ICommandReceiver, IDisposable
    {
        // private readonly ConnectionMultiplexer connection;
        private readonly RedisClient client;

        //  private readonly JsonConverter serializer = new JsonConverter();
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisQueue"/> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        public RedisQueue(string configuration)
        {
            this.client = new RedisClient("localhost");
            //   this.connection = ConnectionMultiplexer.Connect("localhost");
        }


        public Task SendAsync(ICommand command, CancellationToken cancellationToken)
        {
            string value = JsonConvert.SerializeObject(command);
            this.client.RPush("queue", value);

            return Task.FromResult(0);
        }

        public Task<ICommand> ReceiveAsync(CancellationToken cancellationToken)
        {
            var result = this.client.RPopLPush("queue", "procedeed");
            var value = JsonConvert.DeserializeObject<ICommand>(result);
            return Task.FromResult(value);
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        public bool IsCompleted
        {
            get { throw new NotImplementedException(); }
        }

        public void Complete()
        {
            throw new NotImplementedException();
        }
    }
}
