namespace Waffle.Commands
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using Waffle.Internal;
    using Waffle.Properties;

    /// <summary>
    /// Default <see cref="ICommandHandlerSelector"/> instance for choosing a <see cref="CommandHandlerDescriptor"/> given a <see cref="HandlerRequest"/>
    /// A different implementation can be registered via the <see cref="ProcessorConfiguration.Services"/>.
    /// </summary>
    public class DefaultCommandHandlerSelector : ICommandHandlerSelector, ICommandHandlerDescriptorProvider
    {
        private readonly Lazy<ConcurrentDictionary<Type, CommandHandlerDescriptor>> handlerInfoCache;

        private readonly CommandHandlerTypeCache commandHandlerTypeCache;

        private readonly ProcessorConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandHandlerSelector"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public DefaultCommandHandlerSelector(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            this.handlerInfoCache = new Lazy<ConcurrentDictionary<Type, CommandHandlerDescriptor>>(this.InitializeHandlerInfoCache);
            this.configuration = configuration;
            this.commandHandlerTypeCache = new CommandHandlerTypeCache(this.configuration);
        }

        /// <summary>
        /// Selects a <see cref="CommandHandlerDescriptor"/> for the given <see cref="HandlerRequest"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>An <see cref="CommandHandlerDescriptor"/> instance.</returns>
        public CommandHandlerDescriptor SelectHandler(CommandHandlerRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            CommandHandlerDescriptor result;
            if (this.handlerInfoCache.Value.TryGetValue(request.MessageType, out result))
            {
                return result;
            }

            ICollection<Type> handlerTypes = this.commandHandlerTypeCache.GetHandlerTypes(request.MessageType);
            if (handlerTypes.Count == 0)
            {
                throw Error.InvalidOperation(Resources.DefaultHandlerSelector_HandlerNotFound, request.MessageType.Name);
            }
            
            throw CreateAmbiguousHandlerException(request.MessageType.Name, handlerTypes);
        }

        /// <summary>
        /// Returns a map, keyed by commandType, of all <see cref="CommandHandlerDescriptor"/> that the selector can select. 
        /// </summary>
        /// <returns>A map of all <see cref="CommandHandlerDescriptor"/> that the selector can select, or null if the selector does not have a well-defined mapping of <see cref="CommandHandlerDescriptor"/>.</returns>
        public IDictionary<Type, CommandHandlerDescriptor> GetHandlerMapping()
        {
            return this.handlerInfoCache.Value;
        }

        private static Exception CreateAmbiguousHandlerException(string commandName, IEnumerable<Type> matchingTypes)
        {
            Contract.Requires(commandName != null);
            Contract.Requires(matchingTypes != null);

            // Generate an exception containing all the handler types
            StringBuilder typeList = new StringBuilder();
            foreach (Type matchedType in matchingTypes)
            {
                typeList.AppendLine();
                typeList.Append(matchedType.FullName);
            }

            return Error.InvalidOperation(Resources.DefaultHandlerSelector_CommandTypeAmbiguous, commandName, typeList, Environment.NewLine);
        }

        private ConcurrentDictionary<Type, CommandHandlerDescriptor> InitializeHandlerInfoCache()
        {
            ConcurrentDictionary<Type, CommandHandlerDescriptor> concurrentDictionary = new ConcurrentDictionary<Type, CommandHandlerDescriptor>();
            HashSet<Type> duplicateHandlers = new HashSet<Type>();
            Dictionary<Type, ILookup<Type, Type>> cache = this.commandHandlerTypeCache.Cache;
            foreach (KeyValuePair<Type, ILookup<Type, Type>> current in cache)
            {
                Type commandType = current.Key;
                foreach (IGrouping<Type, Type> group in current.Value)
                {
                    foreach (Type handlerType in group)
                    {
                        if (concurrentDictionary.Keys.Contains(commandType))
                        {
                            duplicateHandlers.Add(commandType);
                            break;
                        }

                        concurrentDictionary.TryAdd(commandType, new CommandHandlerDescriptor(this.configuration, commandType, handlerType));
                    }
                }
            }

            foreach (Type duplicateHandler in duplicateHandlers)
            {
                CommandHandlerDescriptor descriptor;
                concurrentDictionary.TryRemove(duplicateHandler, out descriptor);
            }

            return concurrentDictionary;
        }
    }
}