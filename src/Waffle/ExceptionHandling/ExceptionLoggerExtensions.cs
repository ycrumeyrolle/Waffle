namespace Waffle.ExceptionHandling
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Internal;

    /// <summary>Provides extension methods for <see cref="IExceptionLogger"/>.</summary>
    public static class ExceptionLoggerExtensions
    {
        /// <summary>Calls an exception logger.</summary>
        /// <param name="logger">The unhandled exception logger.</param>
        /// <param name="context">The exception context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous exception logging operation.</returns>
        public static Task LogAsync(this IExceptionLogger logger, ExceptionContext context, CancellationToken cancellationToken)
        {
            if (logger == null)
            {
                throw Error.ArgumentNull("logger");
            }

            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            ExceptionLoggerContext loggerContext = new ExceptionLoggerContext(context);
            return logger.LogAsync(loggerContext, cancellationToken);
        }
    }
}
