namespace Waffle.Unity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Practices.Unity;
    using Waffle.Commands;
    using Waffle.Dependencies;
    using Waffle.Events;
    using Waffle.Filters;
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
                throw Error.ArgumentNull("configuration");
            }

            ICommandHandlerDescriptorProvider commandDescriptorProvider = configuration.Services.GetCommandHandlerDescriptorProvider();
            IDictionary<Type, CommandHandlerDescriptor> commandDescriptorsMapping = commandDescriptorProvider.GetHandlerMapping();
            foreach (KeyValuePair<Type, CommandHandlerDescriptor> description in commandDescriptorsMapping)
            {
                LifetimeManager lifetime = GetLifetimeManager(description.Value.Lifetime);
                this.container.RegisterType(description.Value.HandlerType, lifetime);
            }

            IEventHandlerDescriptorProvider eventDescriptorProvider = configuration.Services.GetEventHandlerDescriptorProvider();
            IDictionary<Type, EventHandlersDescriptor> eventDescriptorsMapping = eventDescriptorProvider.GetHandlerMapping();
            foreach (KeyValuePair<Type, EventHandlersDescriptor> descriptor in eventDescriptorsMapping)
            {
                foreach (EventHandlerDescriptor eventHandlerDescriptor in descriptor.Value.EventHandlerDescriptors)
                {
                    LifetimeManager lifetime = GetLifetimeManager(eventHandlerDescriptor.Lifetime);
                    this.container.RegisterType(eventHandlerDescriptor.HandlerType, lifetime);
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

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The object is return to caller.")]
        private static LifetimeManager GetLifetimeManager(HandlerLifetime handlerLifetime)
        {
            switch (handlerLifetime)
            {
                case HandlerLifetime.Transient:
                    return new TransientLifetimeManager();

                case HandlerLifetime.PerRequest:
                    return new HierarchicalLifetimeManager();

                case HandlerLifetime.Processor:
                    return new ContainerControlledLifetimeManager();

                default:
                    throw new InvalidEnumArgumentException("handlerLifetime", (int)handlerLifetime, typeof(HandlerLifetime));
            }
        }
    }
}