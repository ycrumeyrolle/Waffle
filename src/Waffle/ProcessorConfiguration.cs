namespace Waffle
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Waffle.Commands;
    using Waffle.Dependencies;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Queuing;
    using Waffle.Services;
    using Waffle.Tracing;

    /// <summary>
    /// Represents the configuration for a processor.
    /// </summary>
    public sealed class ProcessorConfiguration : IDisposable
    {
        private readonly HashSet<IDisposable> resourcesToDispose = new HashSet<IDisposable>();

        private readonly HandlerFilterCollection filters = new HandlerFilterCollection();

        private bool disposed;

        private bool initialized;

        private Action<ProcessorConfiguration> initializer = DefaultInitializer;

        private IDependencyResolver dependencyResolver = EmptyResolver.Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorConfiguration"/> class.
        /// </summary>
        public ProcessorConfiguration()
        {
            this.Services = new DefaultServices(this);
            this.AbortOnInvalidCommand = true;
            this.DefaultHandlerLifetime = HandlerLifetime.Transient;
            this.DefaultQueuePolicy = QueuePolicy.NoQueue;
            this.Properties = new ConcurrentDictionary<object, object>();            
        }

        private ProcessorConfiguration(ProcessorConfiguration configuration, CommandHandlerSettings settings)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(settings != null);
            
            this.filters = configuration.Filters;
            this.dependencyResolver = configuration.DependencyResolver;
            this.DefaultHandlerLifetime = configuration.DefaultHandlerLifetime;
            this.Properties = configuration.Properties;
            this.CommandBroker = configuration.CommandBroker;

            // per-handler settings
            this.Services = settings.Services;
            this.AbortOnInvalidCommand = settings.AbortOnInvalidCommand;
            this.ServiceProxyCreationEnabled = configuration.ServiceProxyCreationEnabled;

            // Use the original configuration's initializer so that its Initialize()
            // will perform the same logic on this clone as on the original.
            this.Initializer = configuration.Initializer;
        }

        /// <summary>
        /// Gets or sets whether the command should be aborted on invalid command.
        /// </summary>
        /// <value><see langword="true"/> if the command should be aborted on invalid command;  false otherwise.</value>
        public bool AbortOnInvalidCommand { get; set; }

        /// <summary>
        /// Gets or sets whether the services created with the Use method of the <see cref="MessageProcessor"/> 
        /// shoud be a proxy.
        /// </summary>
        /// <value><see langword="true"/> if the service should be a proxy ; false otherwise.</value>
        public bool ServiceProxyCreationEnabled { get; set; }

        /// <summary>
        /// Gets the global <see cref="HandlerFilterCollection"/>.
        /// </summary>
        /// <value>The global <see cref="HandlerFilterCollection"/>.</value>
        public HandlerFilterCollection Filters
        {
            get
            {
                return this.filters;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IDependencyResolver"/>.
        /// </summary>
        /// <value>The <see cref="IDependencyResolver"/>.</value>
        public IDependencyResolver DependencyResolver
        {
            get
            {
                return this.dependencyResolver;
            }

            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                this.dependencyResolver = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="ServicesContainer"/>.
        /// </summary>
        /// <value>The <see cref="ServicesContainer"/>.</value>
        public ServicesContainer Services { get; private set; }

        /// <summary>
        /// Gets or sets the action that will perform final initialization 
        /// of the <see cref="ProcessorConfiguration"/> instance before it is used 
        /// to process requests. 
        /// </summary>
        /// <remarks>
        /// The Action returned by this property will be called to perform 
        /// final initialization of an <see cref="ProcessorConfiguration"/> before it is 
        /// used to process a request. 
        /// <para>
        /// The <see cref="ProcessorConfiguration"/> passed to this action should be
        /// considered immutable after the action returns. 
        /// </para>
        /// </remarks>
        /// <value>
        /// The action that will perform final initialization 
        /// of the <see cref="ProcessorConfiguration"/>
        /// </value>
        public Action<ProcessorConfiguration> Initializer
        {
            get
            {
                return this.initializer;
            }

            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                this.initializer = value;
            }
        }

        /// <summary>
        /// Gets or sets the default handler lifetime.
        /// </summary>
        /// <remarks>
        /// This value is <see cref="HandlerLifetime.Transient"/> per default.
        /// </remarks>
        public HandlerLifetime DefaultHandlerLifetime { get; set; }
        
        /// <summary>
        /// Gets or sets the default queue policy.
        /// </summary>
        /// <remarks>
        /// This value is <see cref="QueuePolicy.NoQueue"/> per default.
        /// </remarks>
        public QueuePolicy DefaultQueuePolicy { get; set; }

        /// <summary>
        /// Gets the properties associated with this configuration.
        /// </summary>
        public ConcurrentDictionary<object, object> Properties { get; private set; }

        /// <summary>
        /// Gets the default Command broker.
        /// </summary>
        public CommandRunner CommandBroker { get; set; }

        /// <summary>Invoke the Intializer hook. It is considered immutable from this point forward. It's safe to call this multiple times.</summary>
        public void EnsureInitialized()
        {
            if (this.initialized)
            {
                return;
            }

            this.initialized = true;
            this.Initializer(this);
        }

        /// <summary>
        /// Releases the unmanaged resources that are used by the object and releases the managed resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.Services.Dispose();
                this.DependencyResolver.Dispose();

                if (this.CommandBroker != null)
                {
                    this.CommandBroker.Dispose();
                }

                foreach (IDisposable resource in this.resourcesToDispose)
                {
                    resource.Dispose();
                }
            }
        }

        internal static ProcessorConfiguration ApplyHandlerSettings(CommandHandlerSettings settings, ProcessorConfiguration configuration)
        {
            Contract.Requires(settings != null);
            Contract.Requires(configuration != null);

            if (!settings.IsServiceCollectionInitialized)
            {
                return configuration;
            }

            return new ProcessorConfiguration(configuration, settings);
        }

        /// <summary>
        /// Adds the given <paramref name="resource"/> to a list of resources that will be disposed once the configuration is disposed.
        /// </summary>
        /// <param name="resource">The resource to dispose. Can be <c>null</c>.</param>
        public void RegisterForDispose(IDisposable resource)
        {
            if (resource != null)
            {
                this.resourcesToDispose.Add(resource);
            }
        }

        /// <summary>
        /// Unregister a resource to be disposed once the configuration is disposed.
        /// </summary>
        /// <param name="resource"></param>
        public void UnregisterForDispose(IDisposable resource)
        {
            if (resource != null)
            {
                this.resourcesToDispose.Remove(resource);
            }
        }

        private static void DefaultInitializer(ProcessorConfiguration configuration)
        {
            // Initialize the tracing layer.
            // This must be the last initialization code to execute
            // because it alters the configuration and expects no
            // further changes.  As a default service, we know it
            // must be present.
            Contract.Requires(configuration != null);
            ITraceManager traceManager = configuration.Services.GetTraceManager();
            
            traceManager.Initialize(configuration);
        }
    }
}