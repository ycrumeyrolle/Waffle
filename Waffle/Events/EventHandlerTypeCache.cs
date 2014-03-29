namespace Waffle.Events
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Waffle.Internal;

    internal sealed class EventHandlerTypeCache
    {
        private readonly ProcessorConfiguration configuration;

        private readonly Lazy<Dictionary<Type, ILookup<Type, Type>>> cache;

        public EventHandlerTypeCache(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            this.configuration = configuration;
            this.cache = new Lazy<Dictionary<Type, ILookup<Type, Type>>>(this.InitializeCache);
        }

        internal Dictionary<Type, ILookup<Type, Type>> Cache
        {
            get
            {
                return this.cache.Value;
            }
        }
        
        private static IEnumerable<Tuple<Type, Type>> GetEventHandlerType(Type handlerType)
        {
            Contract.Requires(handlerType != null);

            return handlerType
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetCustomAttributes<HandlerAttribute>(true).Length != 0)
                .Select(i => Tuple.Create(i.GetGenericArguments()[0], handlerType));
        }

        private Dictionary<Type, ILookup<Type, Type>> InitializeCache()
        {
            IAssembliesResolver assembliesResolver = this.configuration.Services.GetAssembliesResolver();
            IEventHandlerTypeResolver eventHandlerTypeResolver = this.configuration.Services.GetEventHandlerTypeResolver();
            ICollection<Type> handlerTypes = eventHandlerTypeResolver.GetEventHandlerTypes(assembliesResolver);
            
            var source = handlerTypes
                .SelectMany(GetEventHandlerType)
                .GroupBy(i => i.Item1, i => i.Item2);
    
            return source.ToDictionary(g => g.Key, g => g.ToLookup(t => t));
        }
    }
}