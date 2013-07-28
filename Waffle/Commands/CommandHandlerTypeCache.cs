namespace Waffle.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Waffle.Internal;

    internal sealed class CommandHandlerTypeCache
    {
        private static readonly Type CommandHandlerInterfaceType2 = typeof(ICommandHandler<,>);

        private static readonly Type CommandHandlerInterfaceType1 = typeof(ICommandHandler<>);

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
                .Where(i => i.IsGenericType && (IsAssignableFromGenericType(CommandHandlerInterfaceType1, i.GetGenericTypeDefinition()) || IsAssignableFromGenericType(CommandHandlerInterfaceType2, i.GetGenericTypeDefinition())))
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