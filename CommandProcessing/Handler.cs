namespace CommandProcessing
{
    using CommandProcessing.Filters;

    /// <summary>
    /// Represents the command handler. 
    /// It is responsible to do the real processing.
    /// </summary>
    public abstract class Handler : IHandler  
    {
        /// <summary>
        /// Gets the <see cref="ICommandProcessor"/>.
        /// </summary>
        /// <value>The command processor.</value>
        public ICommandProcessor Processor { get; set; }
        
        /// <summary>
        /// Gets the <see cref="HandlerContext"/>.
        /// </summary>
        /// <value>The <see cref="HandlerContext"/>.</value>
        public HandlerContext Context { get; set; }
    }
}