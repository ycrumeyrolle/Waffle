namespace Waffle
{
    using System.Diagnostics.CodeAnalysis;
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
        /// <returns>The result of the command.</returns>
        Task<HandlerResponse> ProcessAsync(ICommand command);

        /// <summary>
        /// Publish the event. 
        /// </summary>
        /// <param name="event">The event to publish.</param>
        /// <returns>The <see cref="Task"/> of the event.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "event", Justification = "Reviewed")]
        Task PublishAsync(IEvent @event);
        
        /// <summary>
        /// Asks the the processor to supply a service.
        /// The service will be created by the <see cref="IDependencyResolver"/>.
        /// If the ServiceProxyCreationEnabled is <see langword="true"/>, the service will be a proxy.
        /// </summary>
        /// <typeparam name="TService">The type of the service to supply.</typeparam>
        /// <returns>The service.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Using", Justification = "An other term could be found...")]
        TService Use<TService>() where TService : class;
    }
}