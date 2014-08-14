namespace Waffle.Results
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;

    /// <summary>
    /// Represents an handler result that throws an <see cref="Exception"/>.
    /// </summary>
    public class ExceptionResult : ICommandHandlerResult
    {
        /// <summary>Initializes a new instance of the <see cref="ExceptionResult"/> class.</summary>
        /// <param name="exception">The exception to include in the error.</param>
        /// <param name="request">The request message which led to this result.</param>
        public ExceptionResult(Exception exception, CommandHandlerRequest request)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            this.Exception = exception;
            this.Request = request;
        }

        /// <summary>Gets the exception to include in the error.</summary>
        public Exception Exception { get; private set; }

        /// <summary>Gets the request message which led to this result.</summary>
        public CommandHandlerRequest Request { get; private set; }

        /// <inheritdoc />
        public virtual Task<HandlerResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(this.Execute());
        }

        private HandlerResponse Execute()
        {
            return this.Request.CreateErrorResponse(this.Exception);
        }
    }
}
