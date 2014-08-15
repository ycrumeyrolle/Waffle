namespace Waffle.Events
{
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Results;
    using Waffle.Services;

    /// <summary>
    /// Default implementation of the <see cref="IEventWorker"/>.
    /// </summary>
    public sealed class DefaultEventWorker : IEventWorker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEventWorker"/> class.
        /// </summary>
        /// <param name="configuration"></param>
        public DefaultEventWorker(ProcessorConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public ProcessorConfiguration Configuration { get; private set; }

        /// <summary>
        /// Execute the request via the worker. 
        /// </summary>
        /// <param name="request">The <see cref="EventHandlerRequest"/> to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the execution.</param>
        /// <returns>The <see cref="Task"/> of the event.</returns>
        public async Task PublishAsync(EventHandlerRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            IEventHandlerSelector handlerSelector = this.Configuration.Services.GetEventHandlerSelector();

            EventHandlersDescriptor eventDescriptor = handlerSelector.SelectHandlers(request);

            var descriptors = eventDescriptor.EventHandlerDescriptors;
            for (int i = 0; i < descriptors.Count; i++)
            {
                await InvokeHandlerAsync(descriptors[i], request, cancellationToken);
            }
        }

        private static Task InvokeHandlerAsync(EventHandlerDescriptor descriptor, EventHandlerRequest request, CancellationToken cancellationToken)
        {
            Contract.Requires(descriptor != null);
            Contract.Requires(request != null);

            IEventHandler handler = descriptor.CreateHandler(request);
            if (handler == null)
            {
                throw CreateHandlerNotFoundException(descriptor);
            }

            request.RegisterForDispose(handler, true);
            EventHandlerContext context = new EventHandlerContext(request, descriptor);
            context.Handler = handler;
            handler.EventContext = context;
            EventFilterGrouping filterGrouping = descriptor.GetFilterGrouping();
     
            ServicesContainer servicesContainer = request.Configuration.Services;
            IEventHandlerResult result = new EventHandlerFilterResult(context, servicesContainer, filterGrouping.EventHandlerFilters);

            return result.ExecuteAsync(cancellationToken);
        }

        private static CommandHandlerNotFoundException CreateHandlerNotFoundException(EventHandlerDescriptor descriptor)
        {
            Contract.Requires(descriptor != null);

            return new CommandHandlerNotFoundException(descriptor.MessageType);
        }
    }
}