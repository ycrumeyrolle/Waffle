namespace Waffle
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Properties;

    /// <summary>
    /// Represents an activator for handlers.
    /// </summary>
    /// <typeparam name="THandler"></typeparam>
    public class HandlerActivator<THandler> where THandler : class
    {
        private const string HandlerInstanceKey = "Waffle_HandlerInstance";

        private readonly object cacheKey = new object();

        private Tuple<HandlerDescriptor, Func<THandler>> fastCache;

        private readonly ConcurrentDictionary<Type, Func<THandler>> activatorRepository = new ConcurrentDictionary<Type, Func<THandler>>();

        /// <summary>
        /// Creates the handler specified by <paramref name="descriptor"/> using the given <paramref name="request"/>.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="descriptor">
        /// The andler descriptor.
        /// </param>
        /// <returns>
        /// The handler.
        /// </returns>
        public THandler Create(HandlerRequest request, HandlerDescriptor descriptor)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (descriptor == null)
            {
                throw Error.ArgumentNull("descriptor");
            }

            try
            {
                return this.TryCreate(request, descriptor);
            }
            catch (Exception innerException)
            {
                throw Error.InvalidOperation(innerException, Resources.DefaultHandlerActivator_ErrorCreatingHandler, descriptor.HandlerType.Name);
            }
        }

        /// <summary>
        /// Returns the handler instance from the dependency resolver if there is one registered
        /// else returns the activator that calls the default constructor for the give handlerType.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="descriptor">
        /// The handler descriptor.
        /// </param>
        /// <param name="activator">
        /// The activator.
        /// </param>
        /// <returns>
        /// The handler.
        /// </returns>
        private THandler GetInstanceOrActivator(HandlerRequest request, HandlerDescriptor descriptor, out Func<THandler> activator)
        {
            Contract.Requires(request != null);
            Contract.Requires(descriptor != null);

            // If dependency resolver returns handler object then use it.
            THandler instance = (THandler)request.GetDependencyScope().GetService(descriptor.HandlerType);
            if (instance != null)
            {
                activator = null;
                return instance;
            }

            switch (descriptor.Lifetime)
            {
                case HandlerLifetime.Singleton:
                    activator = this.CreateSingletonActivator(descriptor.HandlerType);
                    return null;

                case HandlerLifetime.PerRequest:
                    activator = CreatePerRequestActivator(request, descriptor);
                    return null;

                case HandlerLifetime.Transient:
                    activator = TypeActivator.Create<THandler>(descriptor.HandlerType);
                    return null;

                default:
                    throw Error.InvalidEnumArgument("descriptor", (int)descriptor.Lifetime, typeof(HandlerLifetime));
            }
        }

        private Func<THandler> CreateSingletonActivator(Type handlerType)
        {
            Func<THandler> activator;
            if (this.activatorRepository.TryGetValue(handlerType, out activator))
            {
                return activator;
            }

            activator = CreateDelegatingActivator(handlerType);
            this.activatorRepository.TryAdd(handlerType, activator);
            return activator;
        }

        private static Func<THandler> CreatePerRequestActivator(HandlerRequest request, HandlerDescriptor descriptor)
        {
            Contract.Requires(request != null);
            Contract.Requires(descriptor != null);

            Func<THandler> activator;
            Dictionary<Type, Func<THandler>> activators;
            if (request.Properties.TryGetValue(HandlerInstanceKey, out activators))
            {
                if (activators.TryGetValue(descriptor.HandlerType, out activator))
                {
                    return activator;
                }

                activator = CreateDelegatingActivator(descriptor.HandlerType);
                activators.Add(descriptor.HandlerType, activator);
                return activator;
            }

            activator = CreateDelegatingActivator(descriptor.HandlerType);
            activators = new Dictionary<Type, Func<THandler>>();
            activators.Add(descriptor.HandlerType, activator);
            request.Properties.Add(HandlerInstanceKey, activators);
            return activator;
        }

        private static Func<THandler> CreateDelegatingActivator(Type handlerType)
        {
            Contract.Requires(handlerType != null);

            Func<THandler> activator = TypeActivator.Create<THandler>(handlerType);
            THandler instance = activator();
            return () => instance;
        }

        private THandler TryCreate(HandlerRequest request, HandlerDescriptor descriptor)
        {
            Contract.Requires(request != null);
            Contract.Requires(descriptor != null);

            Func<THandler> activator;

            if (descriptor.Lifetime != HandlerLifetime.Transient)
            {
                THandler commandHandler = this.GetInstanceOrActivator(request, descriptor, out activator);
                if (commandHandler != null)
                {
                    // we have a handler registered with the dependency resolver for this handler type                      
                    return commandHandler;
                }
            }
            else if (this.fastCache == null)
            {
                // First check in the local fast cache and if not a match then look in the broader 
                // HandlerDescriptor.Properties cache
                THandler commandHandler = this.GetInstanceOrActivator(request, descriptor, out activator);
                if (commandHandler != null)
                {
                    // we have a handler registered with the dependency resolver for this handler type                      
                    return commandHandler;
                }

                Tuple<HandlerDescriptor, Func<THandler>> cacheItem = Tuple.Create(descriptor, activator);
                Interlocked.CompareExchange(ref this.fastCache, cacheItem, null);
            }
            else if (this.fastCache.Item1 == descriptor)
            {
                // If the key matches and we already have the delegate for creating an instance.
                activator = this.fastCache.Item2;
            }
            else
            {
                // If the key doesn't match then lookup/create delegate in the CommandHandlerDescriptor.Properties for
                // that CommandHandlerDescriptor instance
                object result;
                if (descriptor.Properties.TryGetValue(this.cacheKey, out result))
                {
                    activator = (Func<THandler>)result;
                }
                else
                {
                    THandler commandHandler = this.GetInstanceOrActivator(request, descriptor, out activator);
                    if (commandHandler != null)
                    {
                        // we have a handler registered with the dependency resolver for this handler type
                        return commandHandler;
                    }

                    descriptor.Properties.TryAdd(this.cacheKey, activator);
                }
            }

            var instance = activator();

            return instance;
        }
    }
}