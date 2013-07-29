namespace Waffle.Commands
{
    using Waffle.Filters;

    /// <summary>
    /// Represents a function from a result-full command.
    /// </summary>
    /// <param name="handler">The command handler type.</param>
    /// <param name="command">The command.</param>
    /// <param name="context">The handler context.</param>
    public delegate object HandleCommandFunc(ICommandHandler handler, ICommand command, CommandHandlerContext context);
}