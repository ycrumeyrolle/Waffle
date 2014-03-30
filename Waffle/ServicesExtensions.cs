namespace Waffle
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Waffle.Commands;
    using Waffle.Dependencies;
    using Waffle.Events;
    using Waffle.ExceptionHandling;
    using Waffle.Filters;
    using Waffle.Interception;
    using Waffle.Internal;
    using Waffle.Metadata;
    using Waffle.Properties;
    using Waffle.Services;
    using Waffle.Tracing;
    using Waffle.Validation;

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
        /// Gets the <see cref="ICommandHandlerActivator"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ICommandHandlerActivator"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ICommandHandlerActivator"/> service is not registered.</exception>
        public static ICommandHandlerActivator GetHandlerActivator(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<ICommandHandlerActivator>();
        }

        /// <summary>
        /// Gets the <see cref="IEventHandlerActivator"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IEventHandlerActivator"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IEventHandlerActivator"/> service is not registered.</exception>
        public static IEventHandlerActivator GetEventHandlerActivator(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<IEventHandlerActivator>();
        }

        /// <summary>
        /// Gets the <see cref="IAssembliesResolver"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IAssembliesResolver"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IAssembliesResolver"/> service is not registered.</exception>
        public static IAssembliesResolver GetAssembliesResolver(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<IAssembliesResolver>();
        }

        /// <summary>
        /// Gets the <see cref="ICommandHandlerSelector"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ICommandHandlerSelector"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ICommandHandlerSelector"/> service is not registered.</exception>
        public static ICommandHandlerSelector GetHandlerSelector(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<ICommandHandlerSelector>();
        }

        /// <summary>
        /// Gets the <see cref="IEventHandlerSelector"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IEventHandlerSelector"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IEventHandlerSelector"/> service is not registered.</exception>
        public static IEventHandlerSelector GetEventHandlerSelector(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<IEventHandlerSelector>();
        }

        /// <summary>
        /// Gets the <see cref="ICommandHandlerDescriptorProvider"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ICommandHandlerDescriptorProvider"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ICommandHandlerDescriptorProvider"/> service is not registered.</exception>
        public static ICommandHandlerDescriptorProvider GetCommandHandlerDescriptorProvider(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<ICommandHandlerDescriptorProvider>();
        }

        /// <summary>
        /// Gets the <see cref="IEventHandlerDescriptorProvider"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IEventHandlerDescriptorProvider"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IEventHandlerDescriptorProvider"/> service is not registered.</exception>
        public static IEventHandlerDescriptorProvider GetEventHandlerDescriptorProvider(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<IEventHandlerDescriptorProvider>();
        }

        /// <summary>
        /// Gets the <see cref="ICommandHandlerTypeResolver"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ICommandHandlerTypeResolver"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ICommandHandlerTypeResolver"/> service is not registered.</exception>
        public static ICommandHandlerTypeResolver GetCommandHandlerTypeResolver(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<ICommandHandlerTypeResolver>();
        }

        /// <summary>
        /// Gets the <see cref="IEventHandlerTypeResolver"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IEventHandlerTypeResolver"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IEventHandlerTypeResolver"/> service is not registered.</exception>
        public static IEventHandlerTypeResolver GetEventHandlerTypeResolver(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<IEventHandlerTypeResolver>();
        }

        /// <summary>
        /// Gets the <see cref="IProxyBuilder"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IProxyBuilder"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IProxyBuilder"/> service is not registered.</exception>
        public static IProxyBuilder GetProxyBuilder(this ServicesContainer services)
        {
            Contract.Requires(services != null);

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
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<IInterceptionProvider>();
        }
        
        /// <summary>
        /// Gets the <see cref="ModelMetadataProvider"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ModelMetadataProvider"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ModelMetadataProvider"/> service is not registered.</exception>
        public static ModelMetadataProvider GetModelMetadataProvider(this ServicesContainer services)
        {
            Contract.Requires(services != null);

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
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<IModelFlattener>();
        }

        /// <summary>
        /// Gets the <see cref="IPrincipalProvider"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IPrincipalProvider"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IPrincipalProvider"/> service is not registered.</exception>
        public static IPrincipalProvider GetPrincipalProvider(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<IPrincipalProvider>();
        }

        /// <summary>
        /// Gets the <see cref="ICommandWorker"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ICommandWorker"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ICommandWorker"/> service is not registered.</exception>
        public static ICommandWorker GetCommandWorker(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<ICommandWorker>();
        }

        /// <summary>
        /// Gets the <see cref="IEventWorker"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IEventWorker"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IEventWorker"/> service is not registered.</exception>
        public static IEventWorker GetEventWorker(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<IEventWorker>();
        }

        /// <summary>
        /// Gets the <see cref="IMessageProcessor"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IMessageProcessor"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IMessageProcessor"/> service is not registered.</exception>
        public static IMessageProcessor GetProcessor(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<IMessageProcessor>();
        }

        /// <summary>
        /// Gets the <see cref="ITraceManager"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ITraceManager"/> service.</returns>
        public static ITraceManager GetTraceManager(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetService<ITraceManager>();
        }

        /// <summary>
        /// Gets the <see cref="ITraceWriter"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ITraceWriter"/> service.</returns>
        public static ITraceWriter GetTraceWriter(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetService<ITraceWriter>();
        }

        /// <summary>
        /// Gets the list of <see cref="IInterceptor"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IInterceptor"/> services.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IInterceptor"/> services are not registered.</exception>
        public static IInterceptor[] GetInterceptors(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServices<IInterceptor>();
        }

        /// <summary>
        /// Gets the <see cref="ICommandValidator"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ICommandValidator"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ICommandValidator"/> servic is not registered.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Validators", Justification = "False positive.")]
        public static ICommandValidator GetCommandValidator(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServiceOrThrow<ICommandValidator>();
        }
        
        /// <summary>
        /// Gets the list of <see cref="IFilterProvider"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IFilterProvider"/> services.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IFilterProvider"/> services are not registered.</exception>
        public static IFilterProvider[] GetFilterProviders(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServices<IFilterProvider>();
        }

        /// <summary>
        /// Gets the list of <see cref="ModelValidatorProvider"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ModelValidatorProvider"/> services.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ModelValidatorProvider"/> services are not registered.</exception>
        public static ModelValidatorProvider[] GetModelValidatorProviders(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServices<ModelValidatorProvider>();
        }      
        
        /// <summary>
        /// Gets the list of <see cref="IModelValidatorCache"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IModelValidatorCache"/> services.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IModelValidatorCache"/> services are not registered.</exception>
        internal static IModelValidatorCache GetModelValidatorCache(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetService<IModelValidatorCache>();
        }

        /// <summary>
        /// Gets the <see cref="ICommandSender"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ICommandHandlerInvoker"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ICommandHandlerInvoker"/> service is not registered.</exception>
        internal static ICommandSender GetCommandSender(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetService<ICommandSender>();
        }

        /// <summary>
        /// Gets the <see cref="ICommandReceiver"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ICommandReceiver"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ICommandHandlerInvoker"/> service is not registered.</exception>
        internal static ICommandReceiver GetCommandReceiver(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetService<ICommandReceiver>();
        }

        /// <summary>
        /// Gets the <see cref="ICommandHandlerInvoker"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="ICommandHandlerInvoker"/> service.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ICommandHandlerInvoker"/> service is not registered.</exception>
        internal static ICommandHandlerInvoker GetCommandHandlerInvoker(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetService<ICommandHandlerInvoker>();
        }

        /// <summary>
        /// Gets the <see cref="IEventHandlerInvoker"/> service.
        /// </summary>
        /// <param name="services">The <see cref="ServicesContainer"/>.</param>
        /// <returns>The <see cref="IEventHandlerInvoker"/> services.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IEventHandlerInvoker"/> service is not registered.</exception>
        internal static IEventHandlerInvoker GetEventHandlerInvoker(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetService<IEventHandlerInvoker>();
        }
        
        /// <summary>Returns the registered unhandled exception handler, if any.</summary>
        /// <param name="services">The services container.</param>
        /// <returns>
        /// The registered unhandled exception hander, if present; otherwise, <see langword="null"/>.
        /// </returns>
        public static IExceptionHandler GetExceptionHandler(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetService<IExceptionHandler>();
        }

        /// <summary>Returns the collection of registered unhandled exception loggers.</summary>
        /// <param name="services">The services container.</param>
        /// <returns>The collection of registered unhandled exception loggers.</returns>
        public static IEnumerable<IExceptionLogger> GetExceptionLoggers(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServices<IExceptionLogger>();
        }

        internal static T GetServiceOrThrow<T>(this ServicesContainer services) where T : class
        {
            Contract.Requires(services != null);

            T service = services.GetService<T>();
            if (service == null)
            {
                throw Error.InvalidOperation(Resources.DependencyResolverNoService, typeof(T).FullName);
            }

            return service;
        }

        internal static T GetServiceOrThrow<T>(this IDependencyResolver services) where T : class
        {
            Contract.Requires(services != null);

            T service = services.GetService<T>();
            if (service == null)
            {
                throw Error.InvalidOperation(Resources.DependencyResolverNoService, typeof(T).FullName);
            }

            return service;
        }

        public static TService GetService<TService>(this ServicesContainer services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            return (TService)services.GetService(typeof(TService));
        }

        private static TService[] GetServices<TService>(this ServicesContainer services)
        {
            Contract.Requires(services != null);

            return services.GetServices(typeof(TService)).Cast<TService>().ToArray<TService>();
        }
    }
}