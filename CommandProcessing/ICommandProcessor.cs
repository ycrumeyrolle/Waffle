namespace CommandProcessing
{
    using System.Diagnostics.CodeAnalysis;

    public interface ICommandProcessor
    {
        ProcessorConfiguration Configuration { get; }
        
        TResult Process<TCommand, TResult>(TCommand command, HandlerRequest currentRequest) where TCommand : ICommand;

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Using", Justification = "Nom de méthode volontairement proche du mot clé using.")]
        TService Using<TService>() where TService : class;
    }
}