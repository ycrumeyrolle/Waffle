namespace Waffle
{
    using System;
    using Waffle.Commands;
    using Waffle.Validation;

    /// <summary>
    /// Represents the response of a <see cref="ICommandHandler{TCommand}"/>.
    /// </summary>
    public class HandlerResponse
    {
        /// <summary>
        /// Represents an empty response. This field is readonly.
        /// </summary>
        public static readonly HandlerResponse Empty = new HandlerResponse();

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerResponse"/> class. 
        /// </summary>
        private HandlerResponse()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerResponse"/> class. 
        /// </summary>
        /// <param name="request">The request.</param>
        public HandlerResponse(CommandHandlerRequest request)
            : this(request, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerResponse"/> class. 
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="value">The response value.</param>
        public HandlerResponse(CommandHandlerRequest request, object value)
            : this(request, null, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerResponse"/> class representing an exception. 
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="exception">The response exception.</param>
        public HandlerResponse(CommandHandlerRequest request, Exception exception)
            : this(request, exception, null)
        {
        }

        private HandlerResponse(CommandHandlerRequest request, Exception exception, object value)
        {
            this.Request = request;
            this.Exception = exception;
            this.Value = value;
        }

        /// <summary>
        /// Gets the request.
        /// </summary>
        public CommandHandlerRequest Request { get; private set; }

        /// <summary>
        /// Gets the exception. Can be null.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets the response value.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Gets the <see cref="ModelStateDictionary"/> of the corresponding command.
        /// </summary>
        public ModelStateDictionary ModelState
        {
            get { return this.Request != null ? this.Request.ModelState : null; }
        }

        /// <summary>
        /// Attaches the given <paramref name="request"/> to the <paramref name="response"/> if the response does not already
        /// have a pointer to a request.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="request">The request.</param>
        public static void EnsureResponseHasRequest(HandlerResponse response, CommandHandlerRequest request)
        {
            if (response != null && response.Request == null)
            {
                response.Request = request;
            }
        }
    }
}
