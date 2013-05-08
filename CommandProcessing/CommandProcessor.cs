namespace CommandProcessing
{
    using System;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Internal;

    /// <summary>
    /// Represents a processor of commands. 
    /// Its role is to take a command from a client, validate it, and delegate the processing to an handler.
    /// Then it returns the result to the client.
    /// </summary>
    public sealed class CommandProcessor : ICommandProcessor, IDisposable
    {
        private bool disposed;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandProcessor"/> class. 
        /// </summary>
        public CommandProcessor()
            : this(new ProcessorConfiguration())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandProcessor"/> class. 
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public CommandProcessor(ProcessorConfiguration configuration)
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
        /// <typeparam name="TCommand">The type of command to process.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="command">The command to process.</param>
        /// <returns>The result of the command.</returns>
        public TResult Process<TCommand, TResult>(TCommand command) where TCommand : ICommand
        {
            return this.Process<TCommand, TResult>(command, null);
        }

        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <typeparam name="TCommand">The type of command to process.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="command">The command to process.</param>
        /// <param name="currentRequest">The current request. Pass null if there is not parent request.</param>
        /// <returns>The result of the command.</returns>
        internal TResult Process<TCommand, TResult>(TCommand command, HandlerRequest currentRequest) where TCommand : ICommand
        {
            ICommandWorker commandWorker = this.Configuration.Services.GetCommandWorker();

            using (HandlerRequest request = new HandlerRequest(this.Configuration, command, typeof(TResult), currentRequest))
            {
                request.Processor = new CommandProcessorWrapper(this, request);
                return commandWorker.Execute<TCommand, TResult>(request);
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
        
        private void Initialize()
        {
            this.Configuration.Initializer(this.Configuration);
            this.Configuration.Services.Replace(typeof(ICommandProcessor), this);
        }
    }
}