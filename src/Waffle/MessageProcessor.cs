namespace Waffle
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Runtime.ExceptionServices;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Dependencies;
    using Waffle.Events;
    using Waffle.ExceptionHandling;
    using Waffle.Internal;
    using Waffle.Validation;

    /// <summary>
    /// Represents a processor of commands and events. 
    /// Its role is to take a message from a client, validate it, and delegate the processing to an handler.
    /// Then it returns the result to the client.
    /// </summary>
    public sealed class MessageProcessor : IMessageProcessor, IDisposable
    {
        private bool disposed;
        private bool initialized;
        private object initializationLock = new object();
        private object initializationTarget;

        private IExceptionLogger exceptionLogger;
        private IExceptionHandler exceptionHandler;

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
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public ProcessorConfiguration Configuration { get; private set; }

        /// <remarks>This property is internal and settable only for unit testing purposes.</remarks>
        internal IExceptionLogger ExceptionLogger
        {
            get
            {
                if (this.exceptionLogger == null)
                {
                    this.exceptionLogger = ExceptionServices.GetLogger(this.Configuration);
                }

                return this.exceptionLogger;
            }

            set
            {
                this.exceptionLogger = value;
            }
        }

        /// <remarks>This property is internal and settable only for unit testing purposes.</remarks>
        internal IExceptionHandler ExceptionHandler
        {
            get
            {
                if (this.exceptionHandler == null)
                {
                    this.exceptionHandler = ExceptionServices.GetHandler(this.Configuration);
                }

                return this.exceptionHandler;
            }

            set
            {
                this.exceptionHandler = value;
            }
        }

        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <param name="command">The command to process.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The result of the command.</returns>
        public Task<HandlerResponse> ProcessAsync(ICommand command, CancellationToken cancellationToken)
        {
            return this.ProcessAsync(command, cancellationToken, null);
        }

        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <param name="command">The command to process.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <param name="currentRequest">The current request. Pass null if there is not parent request.</param>
        /// <returns>The result of the command.</returns>
        internal async Task<HandlerResponse> ProcessAsync(ICommand command, CancellationToken cancellationToken, HandlerRequest currentRequest)
        {
            if (command == null)
            {
                throw Error.ArgumentNull("command");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            this.EnsureInitialized();

            using (CommandHandlerRequest request = new CommandHandlerRequest(this.Configuration, command, currentRequest))
            {
                request.Processor = new MessageProcessorWrapper(this, request);

                ExceptionDispatchInfo exceptionInfo;

                try
                {
                    if (!ValidateCommand(request) && request.Configuration.AbortOnInvalidCommand)
                    {
                        HandlerResponse reponse = new HandlerResponse(request);
                        return reponse;
                    }

                    ICommandWorker commandWorker = this.Configuration.Services.GetCommandWorker();
                    return await commandWorker.ExecuteAsync(request);
                }
                catch (OperationCanceledException)
                {
                    // Propogate the canceled task without calling exception loggers or handlers.   
                    throw;
                }
                catch (HandlerResponseException exception)
                {
                    return exception.Response;
                }
                catch (Exception exception)
                {
                    exceptionInfo = ExceptionDispatchInfo.Capture(exception);
                }

                ExceptionContext exceptionContext = new ExceptionContext(exceptionInfo, ExceptionCatchBlocks.MessageProcessor, request);
                await this.ExceptionLogger.LogAsync(exceptionContext, request.CancellationToken);
                HandlerResponse response = await this.ExceptionHandler.HandleAsync(exceptionContext, request.CancellationToken);

                if (response == null)
                {
                    exceptionInfo.Throw();
                }

                return response;
            }
        }

        /// <summary>
        /// Publish the event. 
        /// </summary>
        /// <param name="event">The event to publish.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// /// <returns>The result of the event.</returns>
        public Task PublishAsync(IEvent @event, CancellationToken cancellationToken)
        {
            return this.PublishAsync(@event, cancellationToken, null);
        }

        /// <summary>
        /// Publish the event. 
        /// </summary>
        /// <param name="event">The event to publish.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <param name="currentRequest">The current request. Pass null if there is not parent request.</param>
        /// <returns>The result of the event.</returns>
        internal async Task PublishAsync(IEvent @event, CancellationToken cancellationToken, HandlerRequest currentRequest)
        {
            if (@event == null)
            {
                throw Error.ArgumentNull("@event");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (currentRequest == null)
            {
                throw Error.InvalidOperation("The event '{0}' cannot be publish directly. Process a {1} then publish this event into the 'Handle' method.", @event.GetType().Name, typeof(ICommand).Name);
            }

            IEventStore eventStore = this.Configuration.Services.GetServiceOrThrow<IEventStore>();

            await eventStore.StoreAsync(@event, currentRequest.CancellationToken);

            IEventWorker eventWorker = this.Configuration.Services.GetEventWorker();

            using (EventHandlerRequest request = new EventHandlerRequest(this.Configuration, @event, currentRequest))
            {
                request.Processor = new MessageProcessorWrapper(this, request);
                await eventWorker.PublishAsync(request);
            }
        }

        /// <summary>
        /// Asks the the processor to supply a service.
        /// The service will be created by the <see cref="IDependencyResolver"/>.
        /// If the ServiceProxyCreationEnabled is <see langword="true"/>, the service will be a proxy.
        /// </summary>
        /// <typeparam name="TService">The type of the service to supply.</typeparam>
        /// <returns>The service.</returns>
        public TService Use<TService>() where TService : class
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

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

        private void EnsureInitialized()
        {
            LazyInitializer.EnsureInitialized(
                ref this.initializationTarget,
                ref this.initialized,
                ref this.initializationLock,
                delegate
                {
                    this.Initialize();
                    return null;
                });
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "principal", Justification = "Read the thread principal to work around a problem up to .NET 4.5.1 that CurrentPrincipal creates a new instance each time it is read in async code if it has not been queried from the spawning thread.")]
        private void Initialize()
        {
            // Read the thread principal to work around a problem up to .NET 4.5.1 that CurrentPrincipal creates a new instance each time it is read in async    
            // code if it has not been queried from the spawning thread.   
            // ReSharper disable once UnusedVariable
            IPrincipal principal = Thread.CurrentPrincipal;

            this.Configuration.EnsureInitialized();
            this.Configuration.Services.Replace(typeof(IMessageProcessor), this);
        }

        private static bool ValidateCommand(CommandHandlerRequest request)
        {
            Contract.Requires(request != null);
            Contract.Requires(request.Configuration != null);

            ICommandValidator validator = request.Configuration.Services.GetCommandValidator();
            bool valid = validator.Validate(request);

            return valid;
        }
    }
}