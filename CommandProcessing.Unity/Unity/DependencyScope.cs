namespace CommandProcessing.Unity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Represents a scope that is tracked by the Unity container. The scope is
    /// used to keep track of resources that have been provided, so that they can then be
    /// subsequently released when <see cref="IDisposable.Dispose"/> is called.
    /// </summary>
    public class DependencyScope : IDependencyScope
    {
        private readonly IUnityContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyScope"/> class. 
        /// </summary>
        protected DependencyScope(IUnityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            this.container = container;
        }

        /// <summary>
        /// Gets or sets the <see cref="IUnityContainer"/>.
        /// </summary>
        /// <value>The <see cref="IUnityContainer"/>.</value>
        protected IUnityContainer Container
        {
            get
            {
                return this.container;
            }
        }

        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>. Returns <c>null</c>
        /// if the service is not available.
        /// </summary>
        /// <param name="serviceType">The object type.</param>
        /// <returns>The requested object, if found; <c>null</c> otherwise.</returns>    
        public object GetService(Type serviceType)
        {
            if (this.container.IsRegistered(serviceType))
            {
                return this.container.Resolve(serviceType);
            }

            return null;
        }

        /// <summary>
        /// Gets all instances of the given <paramref name="serviceType"/>. Returns an empty
        /// collection if the service is not available.
        /// </summary>
        /// <param name="serviceType">The object type.</param>
        /// <returns>A sequence of instances of the requested <paramref name="serviceType"/>. The sequence
        /// is be empty (not <c>null</c>) if no objects of the given type are available.</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (this.container.IsRegistered(serviceType))
            {
                return this.container.ResolveAll(serviceType);
            }

            return Enumerable.Empty<object>();
        }

        /// <summary>
        /// Releases unmanaged resources used by the <see cref="DependencyScope"/>.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DependencyScope"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources. </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.container != null)
                {
                    this.container.Dispose();
                }
            }
        }

        /// <summary>
        /// Register handlers into the Unity container.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public void RegisterHandlers(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            IHandlerDescriptorProvider descriptorProvider = configuration.Services.GetHandlerDescriptorProvider();
            IDictionary<Type, HandlerDescriptor> descriptorsMapping = descriptorProvider.GetHandlerMapping();
            foreach (KeyValuePair<Type, HandlerDescriptor> description in descriptorsMapping)
            {
                LifetimeManager lifetime = GetLifetimeManager(description.Value.Lifetime);
                this.container.RegisterType(description.Value.HandlerType, lifetime);
            }
        }

        /// <summary>
        /// Creates a new <see cref="DependencyScope"/>.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        protected static DependencyScope CreateScope(IUnityContainer container)
        {
            return new DependencyScope(container);
        }

        private static LifetimeManager GetLifetimeManager(HandlerLifetime handlerLifetime)
        {
            switch (handlerLifetime)
            {
                case HandlerLifetime.Transcient:
                    return new TransientLifetimeManager();

                case HandlerLifetime.PerRequest:
                case HandlerLifetime.Processor:
                    return new ContainerControlledLifetimeManager();

                default:
                    throw new InvalidEnumArgumentException("handlerLifetime", (int)handlerLifetime, typeof(HandlerLifetime));
            }
        }
    }
}