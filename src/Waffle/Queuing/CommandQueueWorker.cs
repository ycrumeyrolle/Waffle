namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Internal;
    using Waffle.Services;

    /// <summary>
    /// Implementation of the <see cref="ICommandWorker"/> 
    /// </summary>
    public class CommandQueueWorker : ICommandWorker
    {
        private readonly ICommandWorker inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandQueueWorker"/> class.
        /// </summary>
        /// <param name="inner">The inner worker.</param>
        public CommandQueueWorker(ICommandWorker inner)
        {
            this.inner = inner;
        }

        /// <inheritsdoc />
        public async Task<HandlerResponse> ExecuteAsync(CommandHandlerRequest request, CancellationToken cancellationToken)
        {
            QueuePolicy policy = GetQueuePolicy(request);
            if (policy == QueuePolicy.NoQueue)
            {
                return await this.inner.ExecuteAsync(request, cancellationToken);
            }

            return await policy.ExecuteAsync(request, cancellationToken);     
        }

        private static QueuePolicy GetQueuePolicy(CommandHandlerRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }
            
            ServicesContainer servicesContainer = request.Configuration.Services;
            ICommandHandlerSelector handlerSelector = servicesContainer.GetHandlerSelector();
            CommandHandlerDescriptor descriptor = handlerSelector.SelectHandler(request);

            return descriptor.QueuePolicy;
        }
    }
}
