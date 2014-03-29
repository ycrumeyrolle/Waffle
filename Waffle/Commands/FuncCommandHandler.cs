namespace Waffle.Commands
{
    using System;
    using System.Threading.Tasks;
    using Waffle.Filters;

    internal class FuncCommandHandler<TCommand> : IAsyncCommandHandler<TCommand> where TCommand : ICommand
    {
        public CommandHandlerContext CommandContext { get; set; }

        public Task HandleAsync(TCommand command)
        {
            // The method will never be called
            throw new NotImplementedException();
        }
    }
}
