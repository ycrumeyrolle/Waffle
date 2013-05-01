namespace CommandProcessing
{    
    /// <summary>
    /// Represents the command handler. 
    /// It is responsible to do the real processing.
    /// </summary>
    /// <remarks>
    /// This override is a result-less handler.
    /// </remarks>
    /// <typeparam name="TCommand">The command type.</typeparam>
    public abstract class Handler<TCommand> : Handler, IHandler<TCommand, VoidResult>
        where TCommand : ICommand
    {
        VoidResult IHandler<TCommand, VoidResult>.Handle(TCommand command)
        {
            this.Handle(command);
            return null;
        }
        
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        public abstract void Handle(TCommand command);

        public sealed override object Handle(ICommand command)
        {
            this.Handle((TCommand)command);
            return null;
        }
    }
}