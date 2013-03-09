namespace CommandProcessing
{
    public abstract class Handler<TCommand, TResult> : ICommandHandler<TCommand, TResult>
        where TCommand : ICommand
    {
        public CommandProcessor Processor { get; internal set; }

        public abstract TResult Handle(TCommand command);

        object ICommandHandler.Handle(ICommand command)
        {
            return this.Handle((TCommand)command);
        }
    }
}