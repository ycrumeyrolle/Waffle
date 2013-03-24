namespace CommandProcessing
{
    public abstract class Handler<TCommand> : Handler, IHandler<TCommand, EmptyResult>
        where TCommand : ICommand
    {
        EmptyResult IHandler<TCommand, EmptyResult>.Handle(TCommand command)
        {
            this.Handle(command);
            return EmptyResult.Instance;
        }
        
        public abstract void Handle(TCommand command);

        public override object Handle(ICommand command)
        {
            return ((IHandler<TCommand, EmptyResult>)this).Handle((TCommand)command);
        }
    }
}