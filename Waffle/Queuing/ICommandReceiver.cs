namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    
    /// <summary>
    /// Represents a receiver of commands.
    /// </summary>
    public interface ICommandReceiver
    {
        /// <summary>
        /// Gets whether the receiver is completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Receives the commands.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A completion task.</returns>
        Task<ICommand> ReceiveAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Completes the receiver. 
        /// </summary>
        void Complete();
    }
}
