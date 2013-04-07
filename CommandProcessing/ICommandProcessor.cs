namespace CommandProcessing
{
    using System.Diagnostics.CodeAnalysis;
    using CommandProcessing.Dependencies;

    /// <summary>
    /// Represents a processor of commands. 
    /// Its role is to take a command from a client, validate it, and delegate the processing to an handler.
    /// Then it returns the result to the client.
    /// </summary>
    public interface ICommandProcessor
    {
        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <typeparam name="TCommand">The type of command to process.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="command">The command to process.</param>
        /// <param name="currentRequest">The current request. Pass null if there is not parent request.</param>
        /// <returns>The result of the command.</returns>
        TResult Process<TCommand, TResult>(TCommand command, HandlerRequest currentRequest) where TCommand : ICommand;

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