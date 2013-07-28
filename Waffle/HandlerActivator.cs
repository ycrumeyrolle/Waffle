namespace Waffle
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Properties;

    public class HandlerActivator<THandler> where THandler : class
    {
        private readonly object cacheKey = new object();

        private Tuple<HandlerDescriptor, Func<THandler>> fastCache;

        /// <summary>
        /// Creates the <see cref="THandler"/> specified by <paramref name="descriptor"/> using the given <paramref name="request"/>.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="descriptor">
        /// The andler descriptor.
        /// </param>
        /// <returns>
        /// The <see cref="THandler"/>.
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
        /// The <see cref="THandler"/>.
        /// </returns>
        private static THandler GetInstanceOrActivator(HandlerRequest request, HandlerDescriptor descriptor, out Func<THandler> activator)
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
                case HandlerLifetime.Transient:
                case HandlerLifetime.PerRequest:
                case HandlerLifetime.Processor:
                    // Otherwise create a delegate for creating a new instance of the type
                    activator = TypeActivator.Create<THandler>(descriptor.HandlerType);
                    break;

                default:
                    throw Error.InvalidEnumArgument("descriptor", (int)descriptor.Lifetime, typeof(HandlerLifetime));
            }

            return null;
        }

        private THandler TryCreate(HandlerRequest request, HandlerDescriptor descriptor)
        {
            Contract.Requires(request != null);
            Contract.Requires(descriptor != null);

            Func<THandler> activator;

            // First check in the local fast cache and if not a match then look in the broader 
            // CommandHandlerDescriptor.Properties cache
            if (this.fastCache == null)
            {
                THandler commandHandler = GetInstanceOrActivator(request, descriptor, out activator);
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
                    THandler commandHandler = GetInstanceOrActivator(request, descriptor, out activator);
                    if (commandHandler != null)
                    {
                        // we have a handler registered with the dependency resolver for this handler type
                        return commandHandler;
                    }

                    descriptor.Properties.TryAdd(this.cacheKey, activator);
                }
            }

            return activator();
        }
    }
}