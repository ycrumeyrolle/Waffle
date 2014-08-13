namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;

    /// <summary>
    /// Represents a commands receiver.
    /// </summary>
    public interface ICommandSender
    {
        /// <summary>
        /// Gets whether this <see cref="ICommandSender"/> has been marked as complete for adding and is empty. 
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Sends a command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A completion task.</returns>
        Task SendAsync(ICommand command, CancellationToken cancellationToken);
        
        /// <summary>
        /// Marks the <see cref="ICommandSender"/> instances as not accepting any more additions.
        /// </summary>
        void Complete();
    }
}
