namespace Waffle.Commands
{
    using Waffle.Filters;

    /// <summary>
    /// Represents an action from a result-less command.
    /// </summary>
    /// <param name="handler">The command handler type.</param>
    /// <param name="command">The command.</param>
    /// <param name="context">The handler context.</param>
    public delegate void VoidCommandAction(ICommandHandler handler, ICommand command, CommandHandlerContext context);
}