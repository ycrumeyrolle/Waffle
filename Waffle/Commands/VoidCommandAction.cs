namespace Waffle.Commands
{
    using Waffle.Filters;

    public delegate void VoidCommandAction(ICommandHandler handler, ICommand command, CommandHandlerContext context);
}