#if LOOSE_CQRS
namespace Waffle
{
    using System;
    using Waffle.Commands;
    using Waffle.Internal;
    
    /// <summary>
    /// Represents the response of a <see cref="ICommandHandler{TCommand}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HandlerResponse<T>
    {
        private readonly HandlerResponse response;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerResponse"/> class. 
        /// </summary>
        /// <param name="response">The inner <see cref="HandlerResponse"/>.</param>
        public HandlerResponse(HandlerResponse response)
        {
            if (response == null)
            {
                Error.ArgumentNull("response");
            }

            this.response = response;
        }

        /// <summary>
        /// Gets the request.
        /// </summary>
        public HandlerRequest Request
        {
            get
            {
                return this.response.Request;
            }
        }

        /// <summary>
        /// Gets the raised exception if any.
        /// </summary>
        public Exception Exception
        {
            get
            {
                return this.response.Exception;
            }
        }

        /// <summary>
        /// Gets the response value.
        /// </summary>
        public T Value
        {
            get
            {
                return (T)this.response.Value;
            }
        }
    }
}
#endif
