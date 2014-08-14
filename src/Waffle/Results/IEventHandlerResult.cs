namespace Waffle.Results
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Events;

    /// <summary>Defines a command that asynchronously executes an <see cref="IEventHandler"/>.</summary>
    public interface IEventHandlerResult
    {
        /// <summary>Executes an envent handler asynchronously.</summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous execution operation.</returns>
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
