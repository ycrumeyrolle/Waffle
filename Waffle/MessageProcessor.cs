namespace Waffle
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Dependencies;
    using Waffle.Events;
    using Waffle.Internal;
    using Waffle.Tasks;

    /// <summary>
    /// Represents a processor of commands and events. 
    /// Its role is to take a message from a client, validate it, and delegate the processing to an handler.
    /// Then it returns the result to the client.
    /// </summary>
    public sealed class MessageProcessor : IMessageProcessor, IDisposable
    {
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageProcessor"/> class. 
        /// </summary>
        public MessageProcessor()
        {
            this.Configuration = new ProcessorConfiguration();
            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageProcessor"/> class. 
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public MessageProcessor(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            this.Configuration = configuration;
            this.Initialize();
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public ProcessorConfiguration Configuration { get; private set; }

        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="command">The command to process.</param>
        /// <returns>The result of the command.</returns>
        public Task<TResult> ProcessAsync<TResult>(ICommand command)
        {
            return this.ProcessAsync<TResult>(command, null);
        }

        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="command">The command to process.</param>
        /// <param name="currentRequest">The current request. Pass null if there is not parent request.</param>
        /// <returns>The result of the command.</returns>
        internal Task<TResult> ProcessAsync<TResult>(ICommand command, HandlerRequest currentRequest)
        {
            ICommandWorker commandWorker = this.Configuration.Services.GetCommandWorker();

            using (CommandHandlerRequest request = new CommandHandlerRequest(this.Configuration, command, typeof(TResult), currentRequest))
            {
                request.Processor = new MessageProcessorWrapper(this, request);
                return commandWorker.ExecuteAsync<TResult>(request);
            }
        }

        /// <summary>
        /// Publish the event. 
        /// </summary>
        /// <param name="event">The event to publish.</param>
        /// <returns>The result of the event.</returns>
        public Task PublishAsync(IEvent @event)
        {
            return this.PublishAsync(@event, null);
        }

        /// <summary>
        /// Publish the event. 
        /// </summary>
        /// <param name="event">The event to publish.</param>
        /// <param name="currentRequest">The current request. Pass null if there is not parent request.</param>
        /// <returns>The result of the event.</returns>
        internal Task PublishAsync(IEvent @event, HandlerRequest currentRequest)
        {
            CancellationToken cancellationToken = new CancellationToken();
            IEventStore eventStore = this.Configuration.Services.GetServiceOrThrow<IEventStore>();

            Task storeTask = eventStore.StoreAsync(@event, cancellationToken);
            return storeTask.Then(() => this.ExecutePublishAsync(@event, currentRequest, cancellationToken), cancellationToken);
        }

        private Task ExecutePublishAsync(IEvent @event, HandlerRequest currentRequest, CancellationToken cancellationToken)
        {
            IEventWorker eventWorker = this.Configuration.Services.GetEventWorker();

            using (EventHandlerRequest request = new EventHandlerRequest(this.Configuration, @event, currentRequest))
            {
                request.Processor = new MessageProcessorWrapper(this, request);
                return eventWorker.PublishAsync(request, cancellationToken);
            }
        }

        /// <summary>
        /// Asks the the processor to supply a service.
        /// The service will be created by the <see cref="IDependencyResolver"/>.
        /// If the ServiceProxyCreationEnabled is <c>true</c>, the service will be a proxy.
        /// </summary>
        /// <typeparam name="TService">The type of the service to supply.</typeparam>
        /// <returns>The service.</returns>
        public TService Using<TService>() where TService : class
        {
            var service = this.Configuration.DependencyResolver.GetServiceOrThrow<TService>();

            if (this.Configuration.ServiceProxyCreationEnabled)
            {
                var proxyBuilder = this.Configuration.Services.GetProxyBuilder();
                var interceptorProvider = this.Configuration.Services.GetInterceptorProvider();

                service = proxyBuilder.Build(service, interceptorProvider);
            }

            return service;
        }

        /// <summary>
        /// Releases the unmanaged resources that are used by the object and releases the managed resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.Configuration.Dispose();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "principal", Justification = "Read the thread principal to work around a problem up to .NET 4.5.1 that CurrentPrincipal creates a new instance each time it is read in async code if it has not been queried from the spawning thread.")]
        private void Initialize()
        {
             // Read the thread principal to work around a problem up to .NET 4.5.1 that CurrentPrincipal creates a new instance each time it is read in async    
            // code if it has not been queried from the spawning thread.   
            // ReSharper disable once UnusedVariable
            IPrincipal principal = Thread.CurrentPrincipal;  

            this.Configuration.Initializer(this.Configuration);
            this.Configuration.Services.Replace(typeof(IMessageProcessor), this);
        }
    }
}