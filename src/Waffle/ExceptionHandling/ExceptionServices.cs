namespace Waffle.ExceptionHandling
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Waffle.Internal;
    using Waffle.Services;
    
    /// <summary>Creates exception services to call logging and handling from catch blocks.</summary>
    public static class ExceptionServices
    {
        /// <summary>Gets an exception logger that calls all registered logger services.</summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>A composite logger.</returns>
        public static IExceptionLogger GetLogger(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            ServicesContainer services = configuration.Services;
            Contract.Assert(services != null);
            return GetLogger(services);
        }

        /// <summary>Gets an exception logger that calls all registered logger services.</summary>
        /// <param name="services">The services container.</param>
        /// <returns>A composite logger.</returns>
        public static IExceptionLogger GetLogger(ServicesContainer services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            Lazy<IExceptionLogger> exceptionServicesLogger = services.ExceptionServicesLogger;
            Contract.Assert(exceptionServicesLogger != null);
            return exceptionServicesLogger.Value;
        }

        internal static IExceptionLogger CreateLogger(ServicesContainer services)
        {
            Contract.Requires(services != null);

            IEnumerable<IExceptionLogger> loggers = services.GetExceptionLoggers();
            Contract.Assume(loggers != null);
            return new CompositeExceptionLogger(loggers);
        }

        /// <summary>
        /// Gets an exception handler that calls the registered handler service, if any, and ensures exceptions do not
        /// accidentally propagate to the host.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>
        /// An exception handler that calls any registered handler and ensures exceptions do not accidentally propagate
        /// to the host.
        /// </returns>
        public static IExceptionHandler GetHandler(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            ServicesContainer services = configuration.Services;
            Contract.Assert(services != null);
            return GetHandler(services);
        }

        /// <summary>
        /// Gets an exception handler that calls the registered handler service, if any, and ensures exceptions do not
        /// accidentally propagate to the host.
        /// </summary>
        /// <param name="services">The services container.</param>
        /// <returns>
        /// An exception handler that calls any registered handler and ensures exceptions do not accidentally propagate
        /// to the host.
        /// </returns>
        public static IExceptionHandler GetHandler(ServicesContainer services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            Lazy<IExceptionHandler> exceptionServicesHandler = services.ExceptionServicesHandler;
            Contract.Assume(exceptionServicesHandler != null);
            return exceptionServicesHandler.Value;
        }

        internal static IExceptionHandler CreateHandler(ServicesContainer services)
        {
            Contract.Requires(services != null);

            IExceptionHandler innerHandler = services.GetExceptionHandler() ?? new EmptyExceptionHandler();
            return new LastChanceExceptionHandler(innerHandler);
        }
    }
}
