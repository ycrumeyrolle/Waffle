namespace CommandProcessing
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.Serialization;

    [Serializable]
    [ExcludeFromCodeCoverage]
    public class HandlerNotFoundException : Exception
    {
        public HandlerNotFoundException(Type type)
            : this(string.Format(CultureInfo.InvariantCulture, "Command handler not found for command type: {0}", type))
        {
        }

        public HandlerNotFoundException(Type commandType, Type commandResult)
            : this(string.Format(CultureInfo.InvariantCulture, "Command handler not found for command type: {0}, and command result type: {1}", commandType, commandResult))
        {
        }

        public HandlerNotFoundException()
        {
        }

        public HandlerNotFoundException(string message)
            : base(message)
        {
        }

        public HandlerNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected HandlerNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}