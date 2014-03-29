namespace Waffle.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Waffle.Internal;

    internal sealed class CommandHandlerTypeCache
    {
        private readonly ProcessorConfiguration configuration;

        private readonly Lazy<Dictionary<Type, ILookup<Type, Type>>> cache;

        public CommandHandlerTypeCache(ProcessorConfiguration configuration)
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

        public ICollection<Type> GetHandlerTypes(Type commandType)
        {
            if (commandType == null)
            {
                throw Error.ArgumentNull("commandType");
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

        private static IEnumerable<Tuple<Type, Type>> GetCommandType(Type handlerType)
        {
            Contract.Requires(handlerType != null);

            return handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetCustomAttributes<HandlerAttribute>(true).Length != 0)
                .Select(i => Tuple.Create(i.GetGenericArguments()[0], handlerType));
        }

        private Dictionary<Type, ILookup<Type, Type>> InitializeCache()
        {
            IAssembliesResolver assembliesResolver = this.configuration.Services.GetAssembliesResolver();
            ICommandHandlerTypeResolver commandHandlerTypeResolver = this.configuration.Services.GetCommandHandlerTypeResolver();
            ICollection<Type> handlerTypes = commandHandlerTypeResolver.GetCommandHandlerTypes(assembliesResolver);

            var source = handlerTypes
                .SelectMany(GetCommandType)
                .GroupBy(i => i.Item1, i => i.Item2);

            return source.ToDictionary(g => g.Key, g => g.ToLookup(t => t));
        }
    }
}