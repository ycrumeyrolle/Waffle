namespace Waffle.Commands
{
    using System;
    using Waffle.Internal;

    /// <summary>
    /// Represents a request for an handler.    
    /// The <see cref="HandlerRequest"/> is responsible to encapsulate 
    /// all informations around a call to an handler.
    /// </summary>
    public sealed class CommandHandlerRequest : HandlerRequest
    {
        private static readonly Type VoidType = typeof(VoidResult);

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerRequest"/> class. 
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="command">The command.</param>
        public CommandHandlerRequest(ProcessorConfiguration configuration, ICommand command)
            : this(configuration, command, VoidType, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerRequest"/> class. 
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="command">The command.</param>
        /// <param name="parentRequest">The parent request. </param>
        public CommandHandlerRequest(ProcessorConfiguration configuration, ICommand command, HandlerRequest parentRequest)
            : this(configuration, command, VoidType, parentRequest)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerRequest"/> class. 
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="command">The command.</param>
        /// <param name="resultType">The result type.</param>
        public CommandHandlerRequest(ProcessorConfiguration configuration, ICommand command, Type resultType)
            : this(configuration, command, resultType, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerRequest"/> class. 
        /// The request will be a child request of the <paramref name="parentRequest"/>.
        /// </summary> 
        /// <param name="configuration">The configuration.</param>
        /// <param name="command">The command.</param>
        /// <param name="resultType">The result type.</param>
        /// <param name="parentRequest">The parent request. </param>
        public CommandHandlerRequest(ProcessorConfiguration configuration, ICommand command, Type resultType, HandlerRequest parentRequest)
            : base(configuration, parentRequest)
        {
            if (command == null)
            {
                throw Error.ArgumentNull("command");
            }

            if (resultType == null)
            {
                throw Error.ArgumentNull("resultType");
            }

            this.Command = command;
            this.MessageType = command.GetType();
            this.ResultType = resultType;
        }

        /// <summary>
        /// Gets the current <see cref="ICommand"/>.
        /// </summary>
        /// <value>
        /// The <see cref="ICommand"/>.
        /// </value>
        public ICommand Command { get; private set; }

        /// <summary>
        /// Gets the result type.
        /// </summary>
        /// <value>The result <see cref="System.Type"/>.</value>
        public Type ResultType { get; private set; }
    }
}