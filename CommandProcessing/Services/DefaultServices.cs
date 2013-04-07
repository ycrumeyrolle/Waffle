namespace CommandProcessing.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Descriptions;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Eventing;
    using CommandProcessing.Filters;
    using CommandProcessing.Interception;
    using CommandProcessing.Internal;
    using CommandProcessing.Validation;

    /// <summary>
    ///     <para>
    ///         Represents a container for service instances used by the <see cref="ProcessorConfiguration"/>. Note that
    ///         this container only supports known types, and methods to get or set arbitrary service types will
    ///         throw <see cref="ArgumentException"/> when called. For creation of arbitrary types, please use
    ///         <see cref="IDependencyResolver"/> instead. The supported types for this container are:
    ///     </para>
    ///     <list type="bullet">
    ///         <item><see cref="IHandlerSelector"/></item>
    ///         <item><see cref="IHandlerActivator"/></item>
    ///         <item><see cref="IHandlerTypeResolver"/></item>
    ///         <item><see cref="IHandlerNameResolver"/></item>
    ///         <item><see cref="IFilterProvider"/></item>
    ///         <item><see cref="IAssembliesResolver"/></item>
    ///         <item><see cref="ICommandExplorer"/></item>
    ///         <item><see cref="IProxyBuilder"/></item>
    ///         <item><see cref="IInterceptor"/></item>
    ///         <item><see cref="ICommandValidator"/></item>
    ///         <item><see cref="IMessageHub"/></item>
    ///         <item><see cref="ICommandProcessor"/></item>
    ///     </list>
    ///     <para>
    ///         Passing any type which is not on this to any method on this interface will cause
    ///         an <see cref="ArgumentException"/> to be thrown.
    ///     </para>
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Although this class is not sealed, end users cannot set instances of it so in practice it is sealed.")]
    public class DefaultServices : ServicesContainer
    {
        // This lock protects both caches (and _lastKnownDependencyResolver is updated under it as well)
        private readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        private readonly Dictionary<Type, object[]> cacheMulti = new Dictionary<Type, object[]>();

        private readonly Dictionary<Type, object> cacheSingle = new Dictionary<Type, object>();

        private readonly ProcessorConfiguration configuration;

        private readonly HashSet<Type> serviceTypesSingle;

        private readonly HashSet<Type> serviceTypesMulti;

        // Mutation operations delegate (throw if applied to wrong set)
        private readonly Dictionary<Type, object> defaultServicesSingle = new Dictionary<Type, object>();

        private readonly Dictionary<Type, List<object>> defaultServicesMulti = new Dictionary<Type, List<object>>();

        private IDependencyResolver lastKnownDependencyResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultServices"/> class.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="configuration"/> is null.
        /// </exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class needs references to large number of types.")]
        public DefaultServices(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            this.configuration = configuration;

            // Initialize the dictionary with all known service types, even if the list for that service type is
            // empty, because we will throw if the developer tries to read or write unsupported types.
            this.SetSingle<IHandlerSelector>(new DefaultHandlerSelector(this.configuration));
            this.SetSingle<IHandlerActivator>(new DefaultHandlerActivator());
            this.SetSingle<IHandlerTypeResolver>(new DefaultHandlerTypeResolver());
            this.SetSingle<IHandlerNameResolver>(new DefaultHandlerNameResolver());

            this.SetMultiple<IFilterProvider>(new ConfigurationFilterProvider(), new HandlerFilterProvider());

            this.SetSingle<IAssembliesResolver>(new DefaultAssembliesResolver());

            this.SetSingle<ICommandExplorer>(new DefaultCommandExplorer(this.configuration));
            
            this.SetSingle<IProxyBuilder>(new DefaultProxyBuilder());
            this.SetSingle<IInterceptionProvider>(new DefaultInterceptionProvider(this.configuration));
            this.SetMultiple<IInterceptor>(Enumerable.Empty<IInterceptor>().ToArray());
            this.SetMultiple<ICommandValidator>(new DefaultCommandValidator());

            this.SetSingle<IMessageHub>(new MessageHub());
            
            this.SetSingle<ICommandProcessor>(null);

            this.serviceTypesSingle = new HashSet<Type>(this.defaultServicesSingle.Keys);
            this.serviceTypesMulti = new HashSet<Type>(this.defaultServicesMulti.Keys);

            // Reset the caches and the known dependency scope
            this.ResetCache();
        }

        /// <summary>
        /// Queries whether a service type is single-instance.
        /// </summary>
        /// <param name="serviceType">
        /// Type of service to query.
        /// </param>
        /// <returns>
        /// <c>true</c> if the service type has at most one instance, or <c>false</c> if the service type supports multiple instances.
        /// </returns>
        public override bool IsSingleService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            return this.serviceTypesSingle.Contains(serviceType);
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Although this class is not sealed, end users cannot set instances of it so in practice it is sealed.")]
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "Although this class is not sealed, end users cannot set instances of it so in practice it is sealed.")]
        public override void Dispose()
        {
            this.cacheLock.Dispose();
        }

        /// <summary>
        /// Try to get a service of the given type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The first instance of the service, or null if the service is not found.</returns>
        public override object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            if (!this.serviceTypesSingle.Contains(serviceType))
            {
                throw Error.Argument("serviceType", Resources.DefaultServices_InvalidServiceType, serviceType.Name);
            }

            // Invalidate the cache if the dependency scope has switched
            if (this.lastKnownDependencyResolver != this.configuration.DependencyResolver)
            {
                this.ResetCache();
            }

            object result;

            this.cacheLock.EnterReadLock();
            try
            {
                if (this.cacheSingle.TryGetValue(serviceType, out result))
                {
                    return result;
                }
            }
            finally
            {
                this.cacheLock.ExitReadLock();
            }

            // Get the service from DI, outside of the lock. If we're coming up hot, this might
            // mean we end up creating the service more than once.
            object dependencyService = this.configuration.DependencyResolver.GetService(serviceType);

            this.cacheLock.EnterWriteLock();
            try
            {
                if (!this.cacheSingle.TryGetValue(serviceType, out result))
                {
                    result = dependencyService ?? this.defaultServicesSingle[serviceType];
                    this.cacheSingle[serviceType] = result;
                }

                return result;
            }
            finally
            {
                this.cacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Try to get a list of services of the given type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The list of service instances of the given type. Returns an empty enumeration if the
        /// service is not found. </returns>
        public override IEnumerable<object> GetServices(Type serviceType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            if (!this.serviceTypesMulti.Contains(serviceType))
            {
                throw Error.Argument("serviceType", Resources.DefaultServices_InvalidServiceType, serviceType.Name);
            }

            // Invalidate the cache if the dependency scope has switched
            if (this.lastKnownDependencyResolver != this.configuration.DependencyResolver)
            {
                this.ResetCache();
            }

            object[] result;

            this.cacheLock.EnterReadLock();
            try
            {
                if (this.cacheMulti.TryGetValue(serviceType, out result))
                {
                    return result;
                }
            }
            finally
            {
                this.cacheLock.ExitReadLock();
            }

            // Get the service from DI, outside of the lock. If we're coming up hot, this might
            // mean we end up creating the service more than once.
            IEnumerable<object> dependencyServices = this.configuration.DependencyResolver.GetServices(serviceType);

            this.cacheLock.EnterWriteLock();
            try
            {
                if (!this.cacheMulti.TryGetValue(serviceType, out result))
                {
                    result = dependencyServices.Where(s => s != null).Concat(this.defaultServicesMulti[serviceType]).ToArray();
                    this.cacheMulti[serviceType] = result;
                }

                return result;
            }
            finally
            {
                this.cacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Returns the list of object for the given service type. Also validates <paramref name="serviceType"/> is in the known service type list.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The list of service instances of the given type. </returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "inherits from base")]
        protected override List<object> GetServiceInstances(Type serviceType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            List<object> result;
            if (!this.defaultServicesMulti.TryGetValue(serviceType, out result))
            {
                throw Error.Argument("serviceType", Resources.DefaultServices_InvalidServiceType, serviceType.Name);
            }

            return result;
        }

        /// <summary>
        /// Removes a single-instance service from the default services.
        /// </summary>
        /// <param name="serviceType">The type of service.</param>
        protected override void ClearSingle(Type serviceType)
        {
            this.defaultServicesSingle[serviceType] = null;
        }

        /// <summary>
        /// Replaces a single-instance service object.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="service">The service object that replaces the previous instance.</param>
        protected override void ReplaceSingle(Type serviceType, object service)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            this.defaultServicesSingle[serviceType] = service;
        }

        /// <summary>
        /// Removes the cached values for a single service type. Called whenever the user manipulates 
        /// the local service list for a given service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        protected override void ResetCache(Type serviceType)
        {
            this.cacheLock.EnterWriteLock();
            try
            {
                this.cacheSingle.Remove(serviceType);
                this.cacheMulti.Remove(serviceType);
            }
            finally
            {
                this.cacheLock.ExitWriteLock();
            }
        }

        private void SetSingle<T>(T instance) where T : class
        {
            this.defaultServicesSingle[typeof(T)] = instance;
        }

        private void SetMultiple<T>(params T[] instances) where T : class
        {
            IEnumerable<object> x = instances;
            this.defaultServicesMulti[typeof(T)] = new List<object>(x);
        }

        /// <summary>
        /// Removes the cached values for all service types. Called when the dependency scope
        /// has changed since the last time we made a request.
        /// </summary>
        private void ResetCache()
        {
            this.cacheLock.EnterWriteLock();
            try
            {
                this.cacheSingle.Clear();
                this.cacheMulti.Clear();
                this.lastKnownDependencyResolver = this.configuration.DependencyResolver;
            }
            finally
            {
                this.cacheLock.ExitWriteLock();
            }
        }
    }
}