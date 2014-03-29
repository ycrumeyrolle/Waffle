namespace Waffle
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Waffle.Internal;
    using Waffle.Properties;

    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "This type is not meant to be serialized")]
    [SuppressMessage("Microsoft.Usage", "CA2240:Implement ISerializable correctly", Justification = "This type has no serializable state")]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "HandlerResponseException is not a real exception and is just an easy way to return HandlerResponse")]
    public class HandlerResponseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerResponseException"/> class.
        /// </summary>
        /// <param name="response">The response message.</param>
        public HandlerResponseException(HandlerResponse response)
            : base(Resources.HandlerResponseExceptionMessage)
        {
            if (response == null)
            {
                throw Error.ArgumentNull("response");
            }

            this.Response = response;
        }

        /// <summary>
        /// Gets the <see cref="HandlerResponse"/> to return.
        /// </summary>
        public HandlerResponse Response { get; set; }
    }
}
