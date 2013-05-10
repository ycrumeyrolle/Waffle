namespace CommandProcessing.Dispatcher
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;      
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;

    /// <summary>
    /// Default <see cref="IHandlerSelector"/> instance for choosing a <see cref="HandlerDescriptor"/> given a <see cref="HandlerRequest"/>
    /// A different implementation can be registered via the <see cref="ProcessorConfiguration.Services"/>.
    /// </summary>
    public class DefaultHandlerSelector : IHandlerSelector, IHandlerDescriptorProvider
    {
        private static readonly Type VoidType = typeof(void);

        private static readonly Type VoidResultType = typeof(VoidResult);

        private readonly Lazy<ConcurrentDictionary<Type, HandlerDescriptor>> handlerInfoCache;

        private readonly HandlerTypeCache handlerTypeCache;

        private readonly ProcessorConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHandlerSelector"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public DefaultHandlerSelector(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            this.handlerInfoCache = new Lazy<ConcurrentDictionary<Type, HandlerDescriptor>>(this.InitializeHandlerInfoCache);
            this.configuration = configuration;
            this.handlerTypeCache = new HandlerTypeCache(this.configuration);
        }

        /// <summary>
        /// Selects a <see cref="HandlerDescriptor"/> for the given <see cref="HandlerRequest"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>An <see cref="HandlerDescriptor"/> instance.</returns>
        public HandlerDescriptor SelectHandler(HandlerRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            HandlerDescriptor result;
            if (this.handlerInfoCache.Value.TryGetValue(request.CommandType, out result))
            {
                if (!request.ResultType.IsAssignableFrom(result.ResultType) && !(request.ResultType == VoidResultType && result.ResultType == VoidType))
                {
                    throw Error.InvalidOperation(Resources.DefaultHandlerSelector_HandlerNotFound, request.CommandType.Name);
                }

                return result;
            }

            ICollection<Type> handlerTypes = this.handlerTypeCache.GetHandlerTypes(request.CommandType);
            if (handlerTypes.Count == 0)
            {
                throw Error.InvalidOperation(Resources.DefaultHandlerSelector_HandlerNotFound, request.CommandType.Name);
            }
            
            throw CreateAmbiguousHandlerException(request.CommandType.Name, handlerTypes);
        }

        /// <summary>
        /// Returns a map, keyed by commandType, of all <see cref="HandlerDescriptor"/> that the selector can select. 
        /// </summary>
        /// <returns>A map of all <see cref="HandlerDescriptor"/> that the selector can select, or null if the selector does not have a well-defined mapping of <see cref="HandlerDescriptor"/>.</returns>
        public IDictionary<Type, HandlerDescriptor> GetHandlerMapping()
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

        private ConcurrentDictionary<Type, HandlerDescriptor> InitializeHandlerInfoCache()
        {
            ConcurrentDictionary<Type, HandlerDescriptor> concurrentDictionary = new ConcurrentDictionary<Type, HandlerDescriptor>();
            HashSet<Type> duplicateHandlers = new HashSet<Type>();
            Dictionary<Type, ILookup<Type, Type>> cache = this.handlerTypeCache.Cache;
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

                        concurrentDictionary.TryAdd(commandType, new HandlerDescriptor(this.configuration, commandType, handlerType));
                    }
                }
            }

            foreach (Type duplicateHandler in duplicateHandlers)
            {
                HandlerDescriptor descriptor;
                concurrentDictionary.TryRemove(duplicateHandler, out descriptor);
            }

            return concurrentDictionary;
        }
    }
}