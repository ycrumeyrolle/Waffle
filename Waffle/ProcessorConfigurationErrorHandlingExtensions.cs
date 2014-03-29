namespace Waffle
{
    using System.ComponentModel;
    using Waffle.ExceptionHandling;
    using Waffle.Internal;

    /// <summary>
    /// This static class contains helper methods related to the registration
    /// of <see cref="IExceptionHandler"/> instances.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ProcessorConfigurationErrorHandlingExtensions
    {
        /// <summary>
        /// Creates and registers an <see cref="IExceptionHandler"/> implementation to use
        /// for this application. This implementation avoid propagation of exceptions.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/> for which
        /// to register the created exception handler.</param>
        /// <remarks>The returned DefaultExceptionHandler may be further configured to change it's default settings.</remarks>
        /// <returns>The <see cref="DefaultExceptionHandler"/> which was created and registered.</returns>
        public static IExceptionHandler EnableGlobalExceptionHandler(this ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            DefaultExceptionHandler exceptionHandler = new DefaultExceptionHandler();

            configuration.Services.Replace(typeof(IExceptionHandler), exceptionHandler);

            return exceptionHandler;
        }
    }
}