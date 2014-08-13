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
        /// Gets whether this <see cref="ICommandReceiver"/> has been marked as complete for adding and is empty. 
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Receives the commands.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A completion task.</returns>
        Task<ICommand> ReceiveAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Marks the <see cref="ICommandReceiver"/> instances as not accepting any more additions.
        /// </summary>
        void Complete();
    }
}
