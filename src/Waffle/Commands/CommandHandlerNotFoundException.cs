namespace Waffle.Commands
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using Waffle.Internal;
    using Waffle.Properties;

    /// <summary>
    /// Represents an error that occur when no handler is found.
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class CommandHandlerNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerNotFoundException"/> class. 
        /// </summary>
        /// <param name="type"></param>
        public CommandHandlerNotFoundException(Type type)
            : this(Error.Format(Resources.CommandHandlerNotFoundException_VoidType, type))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerNotFoundException"/> class. 
        /// </summary>
        /// <param name="commandType">The type of the command.</param>
        /// <param name="commandResult">The type of the command result.</param>
        public CommandHandlerNotFoundException(Type commandType, Type commandResult)
            : this(Error.Format(Resources.CommandHandlerNotFoundException_ReturnsType, commandType, commandResult))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerNotFoundException"/> class. 
        /// </summary>
        public CommandHandlerNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerNotFoundException"/> class. 
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CommandHandlerNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerNotFoundException"/> class. 
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public CommandHandlerNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerNotFoundException"/> class. 
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.
        /// </param>
        protected CommandHandlerNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}