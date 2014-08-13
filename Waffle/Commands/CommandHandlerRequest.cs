namespace Waffle.Commands
{
    using Waffle.Internal;
    using Waffle.Validation;

    /// <summary>
    /// Represents a request for an handler.    
    /// The <see cref="HandlerRequest"/> is responsible to encapsulate 
    /// all informations around a call to an handler.
    /// </summary>
    public sealed class CommandHandlerRequest : HandlerRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerRequest"/> class. 
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="command">The command.</param>
        public CommandHandlerRequest(ProcessorConfiguration configuration, ICommand command)
            : this(configuration, command, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerRequest"/> class. 
        /// The request will be a child request of the <paramref name="parentRequest"/>.
        /// </summary> 
        /// <param name="configuration">The configuration.</param>
        /// <param name="command">The command.</param>
        /// <param name="parentRequest">The parent request. </param>
        public CommandHandlerRequest(ProcessorConfiguration configuration, ICommand command, HandlerRequest parentRequest)
            : base(configuration, parentRequest)
        {
            if (command == null)
            {
                throw Error.ArgumentNull("command");
            }
            
            this.Command = command;
            this.MessageType = command.GetType();
            this.ModelState = new ModelStateDictionary();
        }

        /// <summary>
        /// Gets the current <see cref="ICommand"/>.
        /// </summary>
        /// <value>
        /// The <see cref="ICommand"/>.
        /// </value>
        public ICommand Command { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ModelStateDictionary"/> of the current request.
        /// </summary>
        /// <value>
        /// The ModelState.
        /// </value>
        public ModelStateDictionary ModelState { get; private set; }
    }
}