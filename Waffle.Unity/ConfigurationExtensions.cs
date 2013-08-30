namespace Waffle
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Practices.Unity;
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Unity;

    /// <summary>
    /// This provides an easy way to handle Unity with the <see cref="MessageProcessor"/>.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Registers the Unity <see cref="DependencyResolver"/> as dependency resolver for the <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="container">The Unity container to register.</param>
        public static DependencyResolver RegisterContainer(this ProcessorConfiguration configuration, IUnityContainer container)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            DependencyResolver dependencyResolver = new DependencyResolver(container);
            configuration.DependencyResolver = dependencyResolver;
            configuration.RegisterHandlers(container);
            return dependencyResolver;
        }

        /// <summary>
        /// Register handlers into the Unity container.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="container">The Unity container.</param>
        public static void RegisterHandlers(this ProcessorConfiguration configuration, IUnityContainer container)
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
                container.RegisterType(description.Value.HandlerType, lifetime);
            }

            IEventHandlerDescriptorProvider eventDescriptorProvider = configuration.Services.GetEventHandlerDescriptorProvider();
            IDictionary<Type, EventHandlersDescriptor> eventDescriptorsMapping = eventDescriptorProvider.GetHandlerMapping();
            foreach (KeyValuePair<Type, EventHandlersDescriptor> descriptor in eventDescriptorsMapping)
            {
                foreach (EventHandlerDescriptor eventHandlerDescriptor in descriptor.Value.EventHandlerDescriptors)
                {
                    LifetimeManager lifetime = GetLifetimeManager(eventHandlerDescriptor.Lifetime);
                    container.RegisterType(eventHandlerDescriptor.HandlerType, lifetime);
                }
            }
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
