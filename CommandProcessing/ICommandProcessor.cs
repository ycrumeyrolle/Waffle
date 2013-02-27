namespace CommandProcessing
{
    public interface ICommandProcessor
    {
        ProcessorConfiguration Configuration { get; }

        void Process<TCommand>(TCommand command) where TCommand : ICommand;

        TResult Process<TCommand, TResult>(TCommand command) where TCommand : ICommand;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Using", Justification = "Nom de méthode volontairement proche du mot clé using.")]
        TService Using<TService>() where TService : class;
    }
}