namespace CommandProcessing
{
    using System;
    using System.Collections.Generic;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;
    using CommandProcessing.Services;

    /// <summary>
    /// Represents the configuration for a processor.
    /// </summary>
    public class ProcessorConfiguration : IDisposable
    {
        private readonly List<IDisposable> resourcesToDispose = new List<IDisposable>();
        
        private readonly HandlerFilterCollection filters = new HandlerFilterCollection();
        
        private bool disposed; 

        private Action<ProcessorConfiguration> initializer = DefaultInitializer;

        private IDependencyResolver dependencyResolver = EmptyResolver.Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorConfiguration"/> class.
        /// </summary>
        public ProcessorConfiguration()
        {
            this.Services = new DefaultServices(this);
            this.AbortOnInvalidCommand = true;
        }

        private ProcessorConfiguration(ProcessorConfiguration configuration, HandlerSettings settings)
        {
            this.filters = configuration.Filters;
            this.dependencyResolver = configuration.DependencyResolver;

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
        /// <value><c>true</c> if the command should be aborted on invalid command;  false otherwise.</value>
        public bool AbortOnInvalidCommand { get; set; }
     
        /// <summary>
        /// Gets or sets whether the services created with the Using method of the <see cref="CommandProcessor"/> 
        /// shoud be a proxy.
        /// </summary>
        /// <value><c>true</c> if the service should be a proxy ; false otherwise.</value>
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
        /// Releases the unmanaged resources that are used by the object and releases the managed resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal static ProcessorConfiguration ApplyHandlerSettings(HandlerSettings settings, ProcessorConfiguration configuration)
        {
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
        internal void RegisterForDispose(IDisposable resource)
        {
            if (resource != null)
            {
                this.resourcesToDispose.Add(resource);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources that are used by the object and, optionally, releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;
                if (disposing)
                {
                    this.Services.Dispose();
                    this.DependencyResolver.Dispose();

                    foreach (IDisposable resource in this.resourcesToDispose)
                    {
                        resource.Dispose();
                    }
                }
            }
        }

        private static void DefaultInitializer(ProcessorConfiguration configuration)
        {
        }
    }
}