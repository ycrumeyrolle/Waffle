namespace Waffle
{
    using System;
    using Waffle.Commands;
    using Waffle.Validation;

    public class HandlerResponse
    {
        internal static readonly HandlerResponse Empty = new HandlerResponse();

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// For testing purpose.
        /// </remarks>
        public HandlerResponse()
        {
        }

        public HandlerResponse(CommandHandlerRequest request)
            : this(request, null, null)
        {
        }

        public HandlerResponse(CommandHandlerRequest request, object value)
            : this(request, null, value)
        {
        }

        public HandlerResponse(CommandHandlerRequest request, Exception exception)
            : this(request, exception, null)
        {
        }

        public HandlerResponse(CommandHandlerRequest request, Exception exception, object value)
        {
            this.Request = request;
            this.Exception = exception;
            this.Value = value;
        }

        public CommandHandlerRequest Request { get; protected set; }

        public Exception Exception { get; protected set; }

        public object Value { get; set; }

        public ModelStateDictionary ModelState
        {
            get { return this.Request.ModelState; }
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
