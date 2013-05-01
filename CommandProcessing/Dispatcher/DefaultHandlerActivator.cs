namespace CommandProcessing.Dispatcher
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;

    /// <summary>
    /// Default implementation of an <see cref="IHandlerActivator"/>.
    /// A different implementation can be registered via the <see cref="T:CommandProcessing.Dependencies.IDependencyResolver"/>.
    /// </summary>
    public class DefaultHandlerActivator : IHandlerActivator
    {
        private readonly object cacheKey = new object();

        private Tuple<HandlerDescriptor, Func<IHandler>> fastCache;

        /// <summary>
        /// Creates the <see cref="Handler"/> specified by <paramref name="descriptor"/> using the given <paramref name="request"/>.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="descriptor">
        /// The andler descriptor.
        /// </param>
        /// <returns>
        /// The <see cref="Handler"/>.
        /// </returns>
        public IHandler Create(HandlerRequest request, HandlerDescriptor descriptor)
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
        /// <param name="handlerType">
        /// The handler Type.
        /// </param>
        /// <param name="activator">
        /// The activator.
        /// </param>
        /// <returns>
        /// The <see cref="Handler"/>.
        /// </returns>
        private static IHandler GetInstanceOrActivator(HandlerRequest request, Type handlerType, out Func<IHandler> activator)
        {
            Contract.Requires(request != null);
            Contract.Requires(handlerType != null);

            // If dependency resolver returns handler object then use it.
            IHandler instance = (IHandler)request.GetDependencyScope().GetService(handlerType);
            if (instance != null)
            {
                activator = null;
                return instance;
            }

            // Otherwise create a delegate for creating a new instance of the type
            activator = TypeActivator.Create<IHandler>(handlerType);
            return null;
        }

        private IHandler TryCreate(HandlerRequest request, HandlerDescriptor descriptor)
        {
            Contract.Requires(request != null);
            Contract.Requires(descriptor != null);

            Func<IHandler> activator;

            // First check in the local fast cache and if not a match then look in the broader 
            // HandlerDescriptor.Properties cache
            if (this.fastCache == null)
            {
                IHandler handler = GetInstanceOrActivator(request, descriptor.HandlerType, out activator);
                if (handler != null)
                {
                    // we have a handler registered with the dependency resolver for this handler type                      
                    return handler;
                }

                Tuple<HandlerDescriptor, Func<IHandler>> cacheItem = Tuple.Create(descriptor, activator);
                Interlocked.CompareExchange(ref this.fastCache, cacheItem, null);
            }
            else if (this.fastCache.Item1 == descriptor)
            {
                // If the key matches and we already have the delegate for creating an instance.
                activator = this.fastCache.Item2;
            }
            else
            {
                // If the key doesn't match then lookup/create delegate in the HandlerDescriptor.Properties for
                // that HandlerDescriptor instance
                object result;
                if (descriptor.Properties.TryGetValue(this.cacheKey, out result))
                {
                    activator = (Func<IHandler>)result;
                }
                else
                {
                    IHandler handler = GetInstanceOrActivator(request, descriptor.HandlerType, out activator);
                    if (handler != null)
                    {
                        // we have a handler registered with the dependency resolver for this handler type
                        return handler;
                    }

                    descriptor.Properties.TryAdd(this.cacheKey, activator);
                }
            }

            return activator();
        }
    }
}