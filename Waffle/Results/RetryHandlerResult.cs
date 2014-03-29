namespace Waffle.Results
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Retrying;

    /// <summary>
    /// Defines a command that asynchronously executes an <see cref="ICommandHandler"/> with a retry policy.
    /// </summary>
    public class RetryHandlerResult : ICommandHandlerResult
    {
        private readonly RetryPolicy retryPolicy;
        private readonly ICommandHandlerResult innerResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryHandlerResult"/> class.
        /// </summary>
        /// <param name="retryPolicy">The <see cref="RetryPolicy"/>.</param>
        /// <param name="innerResult">The inner result.</param>
        public RetryHandlerResult(RetryPolicy retryPolicy, ICommandHandlerResult innerResult)
        {
            this.retryPolicy = retryPolicy;
            this.innerResult = innerResult;
        }

        public Task<HandlerResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            return this.retryPolicy.ExecuteAsync(() => this.innerResult.ExecuteAsync(cancellationToken), cancellationToken);
        }
    }
}
