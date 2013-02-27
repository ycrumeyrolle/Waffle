namespace CommandProcessing
{
    using CommandProcessing.Filters;

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