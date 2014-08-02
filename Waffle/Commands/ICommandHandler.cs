namespace Waffle.Commands
{
    using System.Threading.Tasks;
    using Waffle.Filters;

    /// <summary>
    /// Represents a command handler. 
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    [Handler]
    public interface ICommandHandler<in TCommand> : ICommandHandler where TCommand : ICommand
    {
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <returns>The result object.</returns>
        void Handle(TCommand command);
    }

#if LOOSE_CQRS
    /// <summary>
    /// Represents a command handler. 
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    [Handler]
    public interface ICommandHandler<in TCommand, out TResult> : ICommandHandler where TCommand : ICommand
    {
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The command to process.</param>
        /// <returns>The result object.</returns>
        TResult Handle(TCommand command);
    }
#endif

    /// <summary>
    /// Represents a command handler. 
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>
        /// Gets or sets the current <see cref="CommandHandlerContext"/>
        /// </summary>
        CommandHandlerContext CommandContext { get; set; }
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