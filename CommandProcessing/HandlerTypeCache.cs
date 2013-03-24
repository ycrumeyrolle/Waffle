namespace CommandProcessing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Services;

    internal sealed class HandlerTypeCache
    {
        private static readonly Type CommandHandlerType = typeof(Handler<,>);

        private static readonly Type CommandHandlerInterfaceType = typeof(IHandler<,>);

        private readonly ProcessorConfiguration configuration;

        private readonly Lazy<Dictionary<Type, ILookup<Type, Type>>> cache;

        public HandlerTypeCache(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
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

        public ICollection<Type> GetHandlerTypes(Type commandType)
        {
            if (commandType == null)
            {
                throw new ArgumentNullException("commandType");
            }

            HashSet<Type> matchingTypes = new HashSet<Type>();
            ILookup<Type, Type> lookup;
            if (this.cache.Value.TryGetValue(commandType, out lookup))
            {
                foreach (IGrouping<Type, Type> current in lookup)
                {
                    matchingTypes.UnionWith(current);
                }
            }

            return matchingTypes;
        }

        private static bool IsAssignableFromGenericType(Type givenType, Type genericType)
        {
            if (genericType == null)
            {
                return false;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            Type baseType = givenType.BaseType;
            if (baseType == null)
            {
                return false;
            }

            return IsAssignableFromGenericType(baseType, genericType);
        }

        private static IEnumerable<Tuple<Type, Type>> GetCommandType(Type handlerType)
        {
            return handlerType
                .GetInterfaces()
                .Where(i => i.IsGenericType && IsAssignableFromGenericType(CommandHandlerInterfaceType, i.GetGenericTypeDefinition()))
                .Select(i => Tuple.Create(i.GetGenericArguments()[0], handlerType));
        }

        private Dictionary<Type, ILookup<Type, Type>> InitializeCache()
        {
            IAssembliesResolver assembliesResolver = this.configuration.Services.GetAssembliesResolver();
            IHandlerTypeResolver handlerTypeResolver = this.configuration.Services.GetHandlerTypeResolver();
            ICollection<Type> handlerTypes = handlerTypeResolver.GetHandlerTypes(assembliesResolver);
            
            var source = handlerTypes
                .SelectMany(GetCommandType)
                .GroupBy(i => i.Item1, i => i.Item2);
    
            return source.ToDictionary(g => g.Key, g => g.ToLookup(t => t));
        }
    }
}