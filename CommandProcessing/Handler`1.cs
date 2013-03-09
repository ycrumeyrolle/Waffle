namespace CommandProcessing
{
    public abstract class Handler<TCommand> : ICommandHandler<TCommand, EmptyResult>
        where TCommand : ICommand
    {
        public CommandProcessor Processor { get; internal set; }

        public abstract void Handle(TCommand command);

        EmptyResult ICommandHandler<TCommand, EmptyResult>.Handle(TCommand command)
        {
            this.Handle(command);
            return EmptyResult.Instance;
        }

        object ICommandHandler.Handle(ICommand command)
        {
            this.Handle((TCommand)command);

            return EmptyResult.Instance;
        }
    }
}