namespace CommandProcessing
{
    using System.Security.Principal;
    using CommandProcessing.Filters;

    /// <summary>
    /// Represents the command handler. 
    /// It is responsible to do the real processing.
    /// </summary>
    public abstract class Handler
    {
        /// <summary>
        /// Gets the <see cref="ICommandProcessor"/>.
        /// </summary>
        /// <value>The command processor.</value>
        protected internal ICommandProcessor Processor { get; internal set; }

        /// <summary>
        /// Gets the <see cref="HandlerContext"/>.
        /// </summary>
        /// <value>The <see cref="HandlerContext"/>.</value>
        protected internal HandlerContext Context { get; internal set; }

        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <returns>The result object.</returns>
        public abstract object Handle(ICommand command);

        /// <summary>
        /// Gets the <see cref="IPrincipal"/>.
        /// </summary>
        /// <value>The <see cref="IPrincipal"/>.</value>
        public IPrincipal User
        {
            get
            {
                return this.Context.User;
            }
        }
    }
}