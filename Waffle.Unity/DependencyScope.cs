namespace Waffle.Unity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Practices.Unity;
    using Waffle.Dependencies;
    using Waffle.Internal;

    /// <summary>
    /// Represents a scope that is tracked by the Unity container. The scope is
    /// used to keep track of resources that have been provided, so that they can then be
    /// subsequently released when <see cref="IDisposable.Dispose"/> is called.
    /// </summary>
    public class DependencyScope : IDependencyScope
    {
        private readonly IUnityContainer container;

        // HashSet to avoid UnityContainer Registration filling on IsRegistered method.
        private readonly HashSet<Type> invalidTypes = new HashSet<Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyScope"/> class. 
        /// </summary>
        protected DependencyScope(IUnityContainer container)
        {
            if (container == null)
            {
                throw Error.ArgumentNull("container");
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
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            if (serviceType.IsAbstract && this.invalidTypes.Contains(serviceType) && !this.container.IsRegistered(serviceType))
            {
                return null;
            }

            try
            {
                object service = this.container.Resolve(serviceType);
                return service;
            }
            catch (ResolutionFailedException)
            {
                this.invalidTypes.Add(serviceType);
                return null;
            }
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
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            if (serviceType.IsAbstract && this.invalidTypes.Contains(serviceType) && !this.container.IsRegistered(serviceType))
            {
                return Enumerable.Empty<object>();
            }

            try
            {
                IEnumerable<object> services = this.container.ResolveAll(serviceType);
                return services;
            }
            catch (ResolutionFailedException)
            {
                this.invalidTypes.Add(serviceType);
                return Enumerable.Empty<object>();
            }
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
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources. </param>
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
        /// Creates a new <see cref="DependencyScope"/>.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        protected static DependencyScope CreateScope(IUnityContainer container)
        {
            return new DependencyScope(container);
        }
    }
}