namespace Waffle
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Dependencies;
    using Waffle.Events;

    /// <summary>
    /// Represents a processor of commands and events. 
    /// Its role is to take a message from a client, validate it, and delegate the processing to an handler.
    /// Then it returns the result to the client.
    /// </summary>
    public interface IMessageProcessor
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        ProcessorConfiguration Configuration { get; }

        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <param name="command">The command to process.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The result of the command.</returns>
        Task<HandlerResponse> ProcessAsync(ICommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Publish the event. 
        /// </summary>
        /// <param name="event">The event to publish.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="Task"/> of the event.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "event", Justification = "Reviewed")]
        Task PublishAsync(IEvent @event, CancellationToken cancellationToken);
    }
}