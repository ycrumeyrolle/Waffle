namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;

    public class QueuePolicy
    {
        private static readonly QueuePolicy NoQueueInstance = new QueuePolicy(new NoQueueStrategy());

        private static readonly QueuePolicy QueueInstance = new QueuePolicy(new QueueStrategy());

        private readonly QueueStrategyBase strategy;

        public QueuePolicy(QueueStrategyBase strategy)
        {
            this.strategy = strategy;
        }

        public static QueuePolicy NoQueue
        {
            get
            {
                return NoQueueInstance;
            }
        }

        public static QueuePolicy Queue
        {
            get
            {
                return QueueInstance;
            }
        }

        public Task<HandlerResponse> ExecuteAsync(CommandHandlerRequest request, CancellationToken cancellationToken)
        {
            return this.strategy.ExecuteAsync(request, cancellationToken);
        }
    } 
}
