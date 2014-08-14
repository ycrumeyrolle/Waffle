namespace Waffle.Results
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Represents an action result that returns a specified response message.</summary>
    public class ResponseMessageResult : ICommandHandlerResult
    {
        private readonly HandlerResponse response;

        /// <summary>Initializes a new instance of the <see cref="ResponseMessageResult"/> class.</summary>
        /// <param name="response">The response message.</param>
        public ResponseMessageResult(HandlerResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            this.response = response;
        }

        /// <summary>Gets the response message.</summary>
        public HandlerResponse Response
        {
            get { return this.response; }
        }

        /// <inheritdoc />
        public virtual Task<HandlerResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(this.response);
        }
    }
}
