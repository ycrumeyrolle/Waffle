namespace Waffle.Commands
{
    using Waffle.Filters;

    /// <summary>
    /// Represents a command handler. 
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    public interface ICommandHandler<in TCommand> : ICommandHandler where TCommand : ICommand
    {
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <param name="context">The <see cref="CommandHandlerContext"/>.</param>
        /// <returns>The result object.</returns>
        void Handle(TCommand command, CommandHandlerContext context);
    }

    /// <summary>
    /// Represents a command handler. 
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public interface ICommandHandler<in TCommand, out TResult> : ICommandHandler where TCommand : ICommand
    {
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <param name="context">The <see cref="CommandHandlerContext"/>.</param>
        /// <returns>The result object.</returns>
        TResult Handle(TCommand command, CommandHandlerContext context);
    }

    /// <summary>
    /// Represents a command handler. 
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <param name="context">The <see cref="CommandHandlerContext"/>.</param>
        /// <returns>The result object.</returns>
        object Handle(ICommand command, CommandHandlerContext context);
    }
}