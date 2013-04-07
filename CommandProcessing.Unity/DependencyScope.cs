namespace CommandProcessing.Unity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Services;
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
        public DependencyScope(IUnityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            this.container = container;
        }

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

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

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

            var explorer = configuration.Services.GetCommandExplorer();
            foreach (var description in explorer.Descriptions)
            {
                this.container.RegisterType(description.HandlerType);
            }
        }
    }
}