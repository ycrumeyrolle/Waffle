namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a runner of commands. 
    /// </summary>
    public interface ICommandRunner
    {
        /// <summary>
        /// Start the runner.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>to stop the runner.</param>
        /// <returns>A compeltion task.</returns>
        Task StartAsync(CancellationToken cancellationToken);
    }
}
