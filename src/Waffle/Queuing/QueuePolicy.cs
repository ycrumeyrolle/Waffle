namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;

    /// <summary>
    /// Provides queueing policy mechanism.
    /// </summary>
    public class QueuePolicy
    {
        private static readonly QueuePolicy NoQueueInstance = new QueuePolicy(new NoQueueStrategy());

        private static readonly QueuePolicy QueueInstance = new QueuePolicy(new QueueStrategy());

        private readonly IQueueStrategy strategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuePolicy"/> class.
        /// </summary> 
        /// <param name="strategy">The <see cref="IQueueStrategy"/>.</param>
        public QueuePolicy(IQueueStrategy strategy)
        {
            this.strategy = strategy;
        }

        /// <summary>
        /// Gets the <see cref="M:NoQueue"/> policy.
        /// </summary>
        public static QueuePolicy NoQueue
        {
            get
            {
                return NoQueueInstance;
            }
        }

        /// <summary>
        /// Gets the <see cref="M:Queue"/> policy.
        /// </summary>
        public static QueuePolicy Queue
        {
            get
            {
                return QueueInstance;
            }
        }

        /// <inheritdocs />
        public Task<HandlerResponse> ExecuteAsync(CommandHandlerRequest request, CancellationToken cancellationToken)
        {
            return this.strategy.ExecuteAsync(request, cancellationToken);
        }
    } 
}
