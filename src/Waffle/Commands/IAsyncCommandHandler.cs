namespace Waffle.Commands
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a command handler. 
    /// </summary>
    [Handler]
    public interface IAsyncCommandHandler<in TCommand> : ICommandHandler where TCommand : ICommand
    {
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <returns>The result object.</returns>
        Task HandleAsync(TCommand command);
    }

#if LOOSE_CQRS
    /// <summary>
    /// Represents a command handler. 
    /// </summary>
    [Handler]
    public interface IAsyncCommandHandler<in TCommand, TResult> : ICommandHandler where TCommand : ICommand
    {
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <returns>The result object.</returns>
        Task<TResult> HandleAsync(TCommand command);
    }
#endif
}