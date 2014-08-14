namespace Waffle.Events
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Waffle.Internal;
    using Waffle.Properties;

    /// <summary>
    /// Default <see cref="IEventHandlerSelector"/> instance for choosing a <see cref="EventHandlerDescriptor"/> given a <see cref="HandlerRequest"/>
    /// A different implementation can be registered via the <see cref="ProcessorConfiguration.Services"/>.
    /// </summary>
    public class DefaultEventHandlerSelector : IEventHandlerSelector, IEventHandlerDescriptorProvider
    {
        private readonly Lazy<ConcurrentDictionary<Type, EventHandlersDescriptor>> handlerInfoCache;

        private readonly EventHandlerTypeCache handlerTypeCache;

        private readonly ProcessorConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEventHandlerSelector"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public DefaultEventHandlerSelector(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            this.handlerInfoCache = new Lazy<ConcurrentDictionary<Type, EventHandlersDescriptor>>(this.InitializeHandlerInfoCache);
            this.configuration = configuration;
            this.handlerTypeCache = new EventHandlerTypeCache(this.configuration);
        }

        /// <summary>
        /// Selects a <see cref="EventHandlerDescriptor"/> for the given <see cref="HandlerRequest"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>An <see cref="EventHandlerDescriptor"/> instance.</returns>
        public EventHandlersDescriptor SelectHandlers(EventHandlerRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            EventHandlersDescriptor result;
            if (!this.handlerInfoCache.Value.TryGetValue(request.MessageType, out result))
            {
                throw Error.InvalidOperation(Resources.DefaultHandlerSelector_HandlerNotFound, request.MessageType.Name);
            }

            return result;
        }

        /// <summary>
        /// Returns a map, keyed by eventType, of all <see cref="EventHandlerDescriptor"/> that the selector can select. 
        /// </summary>
        /// <returns>A map of all <see cref="EventHandlerDescriptor"/> that the selector can select, or null if the selector does not have a well-defined mapping of <see cref="EventHandlerDescriptor"/>.</returns>
        public IDictionary<Type, EventHandlersDescriptor> GetHandlerMapping()
        {
            return this.handlerInfoCache.Value;
        }
        
        private ConcurrentDictionary<Type, EventHandlersDescriptor> InitializeHandlerInfoCache()
        {
            ConcurrentDictionary<Type, EventHandlersDescriptor> concurrentDictionary = new ConcurrentDictionary<Type, EventHandlersDescriptor>();
            Dictionary<Type, ILookup<Type, Type>> cache = this.handlerTypeCache.Cache;
            foreach (KeyValuePair<Type, ILookup<Type, Type>> current in cache)
            {
                Type eventType = current.Key;
                Collection<EventHandlerDescriptor> descriptors = new Collection<EventHandlerDescriptor>();
                foreach (IGrouping<Type, Type> group in current.Value)
                {
                    foreach (Type handlerType in group)
                    {
                        EventHandlerDescriptor descriptor = new EventHandlerDescriptor(this.configuration, eventType, handlerType);
                        descriptors.Add(descriptor);
                    }
                }

                EventHandlersDescriptor eventDescriptor = new EventHandlersDescriptor(eventType.Name, descriptors);
                concurrentDictionary.TryAdd(eventType, eventDescriptor);
            }
            
            return concurrentDictionary;
        }
    }
}