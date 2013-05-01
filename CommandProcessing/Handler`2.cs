namespace CommandProcessing
{
    /// <summary>
    /// Represents the command handler. 
    /// It is responsible to do the real processing.
    /// </summary>
    /// <remarks>
    /// This override is a resulting handler.
    /// </remarks>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public abstract class Handler<TCommand, TResult> : Handler, IHandler<TCommand, TResult> where TCommand : ICommand
    {
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <returns>The result object.</returns>
        public abstract TResult Handle(TCommand command);
        
        public sealed override object Handle(ICommand command)
        {
            return this.Handle((TCommand)command);
        }
    }
}