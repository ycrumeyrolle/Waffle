namespace Waffle.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Filters;

    /// <summary>
    /// Provides method to invoke a <see cref="ICommandHandler"/>.
    /// </summary>
    public interface ICommandHandlerInvoker
    {
        /// <summary>
        /// Invokes a <see cref="ICommandHandler"/>.
        /// </summary>
        /// <param name="context">The context of invocation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> of HandlerResponse.</returns>
        Task<HandlerResponse> InvokeHandlerAsync(CommandHandlerContext context, CancellationToken cancellationToken);
    }
}