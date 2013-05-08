namespace CommandProcessing
{
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;

    /// <summary>
    /// Represents the command handler. 
    /// It is responsible to do the real processing.
    /// </summary>
    public abstract class Handler : IHandler
    {
        private const int MaxStackCount = 10;

        private int stackCount;

        /// <summary>
        /// Gets the <see cref="ICommandProcessor"/>.
        /// </summary>
        /// <value>The command processor.</value>
        public ICommandProcessor Processor
        {
            get
            {
                if (this.Context != null && this.Context != null)
                {
                    return this.Context.Request.Processor;
                }

                return null;
            }
        }
        
        /// <summary>
        /// Gets the <see cref="HandlerContext"/>.
        /// </summary>
        /// <value>The <see cref="HandlerContext"/>.</value>
        public HandlerContext Context { get; set; }
        
        public virtual object Handle(ICommand command)
        {
            if (command == null)
            {
                throw Error.ArgumentNull("command");
            }

            // Avoid StackOverflowException in case of incorrect invocation.
            // This could happen when services are badly reimplemented.
            // Handlers are instanciated for each call, so it is not needed to reset the counter.
            if (this.stackCount++ >= MaxStackCount)
            {
                throw Error.Argument("command", "Dynamic invoke of the Handle method failed. This is probably because of incorrect the handler '{0}' has no Handle method with parameter of type '{1}'.", this.GetType().Name, command.GetType().Name);
            }

            return ((dynamic)this).Handle((dynamic)command);
        }
    }
}