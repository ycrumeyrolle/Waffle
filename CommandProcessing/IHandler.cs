namespace CommandProcessing
{
    using CommandProcessing.Filters;

    /// <summary>
    /// Represents a command handler. 
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public interface IHandler<in TCommand, out TResult> : IHandler where TCommand : ICommand
    {
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <returns>The result object.</returns>
        TResult Handle(TCommand command);
    }  
    
    /// <summary>
    /// Represents a command handler. 
    /// </summary>
    public interface IHandler
    {
        ICommandProcessor Processor { get; }

        /// <summary>
        /// Gets the <see cref="HandlerContext"/>.
        /// </summary>
        /// <value>The <see cref="HandlerContext"/>.</value>
        HandlerContext Context { get; set; }

        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <returns>The result object.</returns>
        object Handle(ICommand command);
    }
}
