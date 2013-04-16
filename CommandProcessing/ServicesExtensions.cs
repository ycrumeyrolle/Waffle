namespace CommandProcessing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Descriptions;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Eventing;
    using CommandProcessing.Filters;
    using CommandProcessing.Interception;
    using CommandProcessing.Internal;
    using CommandProcessing.Metadata;
    using CommandProcessing.Services;
    using CommandProcessing.Validation;

    /// <summary>
    /// This provides a centralized list of type-safe accessors describing where and how we get services.
    /// This also provides a single entry point for each service request. That makes it easy
    /// to see which parts of the code use it, and provides a single place to comment usage.
    /// Accessors encapsulate usage like:
    /// <list type="bullet">
    /// <item>Type-safe using {T} instead of unsafe <see cref="System.Type"/>.</item>
    /// <item>which type do we key off? This is interesting with type hierarchies.</item>
    /// <item>do we ask for singular or plural?</item>
    /// <item>is it optional or mandatory?</item>
    /// <item>what are the ordering semantics</item>
    /// </list>
    /// Expected that any <see cref="IEnumerable{T}"/> we return is non-null, although possibly empty.
    /// </summary>
    public static class ServicesExtensions
    {
        /// <summary>
        /// Gets the <see cref="IHandlerActivator"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IHandlerActivator"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IHandlerActivator"/> service is not registered.</exception>
        public static IHandlerActivator GetHandlerActivator(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IHandlerActivator>();
        }

        /// <summary>
        /// Gets the <see cref="IHandlerNameResolver"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IHandlerNameResolver"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IHandlerNameResolver"/> service is not registered.</exception>
        public static IHandlerNameResolver GetHandlerNameResolver(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IHandlerNameResolver>();
        }

        /// <summary>
        /// Gets the <see cref="IAssembliesResolver"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IAssembliesResolver"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IAssembliesResolver"/> service is not registered.</exception>
        public static IAssembliesResolver GetAssembliesResolver(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IAssembliesResolver>();
        }

        /// <summary>
        /// Gets the <see cref="IHandlerSelector"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IHandlerSelector"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IHandlerSelector"/> service is not registered.</exception>
        public static IHandlerSelector GetHandlerSelector(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IHandlerSelector>();
        }

        /// <summary>
        /// Gets the <see cref="IHandlerTypeResolver"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IHandlerTypeResolver"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IHandlerTypeResolver"/> service is not registered.</exception>
        public static IHandlerTypeResolver GetHandlerTypeResolver(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IHandlerTypeResolver>();
        }

        /// <summary>
        /// Gets the <see cref="IProxyBuilder"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IProxyBuilder"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IProxyBuilder"/> service is not registered.</exception>
        public static IProxyBuilder GetProxyBuilder(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IProxyBuilder>();
        }

        /// <summary>
        /// Gets the <see cref="IInterceptionProvider"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IInterceptionProvider"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IInterceptionProvider"/> service is not registered.</exception>
        public static IInterceptionProvider GetInterceptorProvider(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IInterceptionProvider>();
        }

        /// <summary>
        /// Gets the <see cref="IMessageHub"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IMessageHub"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IMessageHub"/> service is not registered.</exception>
        public static IMessageHub GetMessageHub(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IMessageHub>();
        }
        
        /// <summary>
        /// Gets the <see cref="ModelMetadataProvider"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ModelMetadataProvider"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ModelMetadataProvider"/> service is not registered.</exception>
        public static ModelMetadataProvider GetModelMetadataProvider(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<ModelMetadataProvider>();
        }

        /// <summary>
        /// Gets the list of <see cref="IModelFlattener"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IModelFlattener"/> services.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IModelFlattener"/> services are not registered.</exception>
        public static IModelFlattener GetModelFlattener(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IModelFlattener>();
        }

        /// <summary>
        /// Gets the list of <see cref="ICommandExplorer"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ICommandExplorer"/> services.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ICommandExplorer"/> services are not registered.</exception>
        public static ICommandExplorer GetCommandExplorer(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<ICommandExplorer>();
        }

        /// <summary>
        /// Gets the <see cref="IPrincipalProvider"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IPrincipalProvider"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IPrincipalProvider"/> service is not registered.</exception>
        public static IPrincipalProvider GetPrincipalProvider(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IPrincipalProvider>();
        }

        /// <summary>
        /// Gets the <see cref="ICommandProcessor"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ICommandProcessor"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ICommandProcessor"/> service is not registered.</exception>
        public static ICommandProcessor GetProcessor(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<ICommandProcessor>();
        }

        /// <summary>
        /// Gets the list of <see cref="IInterceptor"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IInterceptor"/> services.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IInterceptor"/> services are not registered.</exception>
        public static IEnumerable<IInterceptor> GetInterceptors(this ServicesContainer services)
        {
            return services.GetServices<IInterceptor>();
        }

        /// <summary>
        /// Gets the list of <see cref="ICommandValidator"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ICommandValidator"/> services.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ICommandValidator"/> services are not registered.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Validators", Justification = "False positive.")]
        public static IEnumerable<ICommandValidator> GetCommandValidators(this ServicesContainer services)
        {
            return services.GetServices<ICommandValidator>();
        }

        /// <summary>
        /// Gets the list of <see cref="IFilterProvider"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IFilterProvider"/> services.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IFilterProvider"/> services are not registered.</exception>
        public static IEnumerable<IFilterProvider> GetFilterProviders(this ServicesContainer services)
        {
            return services.GetServices<IFilterProvider>();
        }

        internal static T GetServiceOrThrow<T>(this ServicesContainer services) where T : class
        {
            T service = services.GetService<T>();
            if (service == null)
            {
                throw Error.InvalidOperation(Resources.DependencyResolverNoService, typeof(T).FullName);
            }

            return service;
        }

        internal static T GetServiceOrThrow<T>(this IDependencyResolver services) where T : class
        {
            T service = services.GetService<T>();
            if (service == null)
            {
                throw Error.InvalidOperation(Resources.DependencyResolverNoService, typeof(T).FullName);
            }

            return service;
        }

        private static TService GetService<TService>(this ServicesContainer services)
        {
            return (TService)services.GetService(typeof(TService));
        }

        private static IEnumerable<TService> GetServices<TService>(this ServicesContainer services)
        {
            return services.GetServices(typeof(TService)).Cast<TService>();
        }
    }
}