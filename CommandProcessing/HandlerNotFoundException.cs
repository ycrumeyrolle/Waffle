namespace CommandProcessing
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an error that occur when no handler is found.
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class HandlerNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class. 
        /// </summary>
        /// <param name="type"></param>
        public HandlerNotFoundException(Type type)
            : this(string.Format(CultureInfo.InvariantCulture, "Command handler not found for command type: {0}", type))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class. 
        /// </summary>
        /// <param name="commandType">The type of the command.</param>
        /// <param name="commandResult">The type of the command result.</param>
        public HandlerNotFoundException(Type commandType, Type commandResult)
            : this(string.Format(CultureInfo.InvariantCulture, "Command handler not found for command type: {0}, and command result type: {1}", commandType, commandResult))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class. 
        /// </summary>
        public HandlerNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class. 
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public HandlerNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class. 
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public HandlerNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class. 
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.
        /// </param>
        protected HandlerNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}