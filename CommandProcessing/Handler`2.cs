namespace CommandProcessing
{
    public abstract class Handler<TCommand, TResult> : Handler, IHandler<TCommand, TResult> where TCommand : ICommand
    {
        public abstract TResult Handle(TCommand command);

        public override object Handle(ICommand command)
        {
            return this.Handle((TCommand)command);
        }
    }
}