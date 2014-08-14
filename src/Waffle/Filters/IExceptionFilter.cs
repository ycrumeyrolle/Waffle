namespace Waffle.Filters
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the methods that are required for an exception filter.
    /// </summary>
    public interface IExceptionFilter : IFilter
    {
        /// <summary>
        /// Executes an asynchronous exception filter.
        /// </summary>
        /// <param name="handlerExecutedContext">The handler executed context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous exception filter.</returns>
        Task ExecuteExceptionFilterAsync(CommandHandlerExecutedContext handlerExecutedContext, CancellationToken cancellationToken);
    }
}