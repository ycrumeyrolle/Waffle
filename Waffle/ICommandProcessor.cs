namespace Waffle
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Waffle.Dependencies;

    /// <summary>
    /// Represents a processor of commands. 
    /// Its role is to take a command from a client, validate it, and delegate the processing to an handler.
    /// Then it returns the result to the client.
    /// </summary>
    public interface ICommandProcessor
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        ProcessorConfiguration Configuration { get; }

        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="command">The command to process.</param>
        /// <returns>The result of the command.</returns>
        Task<TResult> ProcessAsync<TResult>(ICommand command);
        
        /// <summary>
        /// Asks the the processor to supply a service.
        /// The service will be created by the <see cref="IDependencyResolver"/>.
        /// If the ServiceProxyCreationEnabled is <c>true</c>, the service will be a proxy.
        /// </summary>
        /// <typeparam name="TService">The type of the service to supply.</typeparam>
        /// <returns>The service.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Using", Justification = "An other term could be found...")]
        TService Using<TService>() where TService : class;
    }
}