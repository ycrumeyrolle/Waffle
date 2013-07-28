namespace Waffle.Commands
{
    using Waffle.Filters;

    public delegate object HandleCommandFunc(ICommandHandler handler, ICommand command, CommandHandlerContext context);
}