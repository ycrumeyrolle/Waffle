namespace Waffle.ExceptionHandling
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Defines an unhandled exception logger.</summary>
    public interface IExceptionLogger
    {
        /// <summary>Logs an unhandled exception.</summary>
        /// <param name="context">The exception logger context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous exception logging operation.</returns>
        Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken);
    }
}
