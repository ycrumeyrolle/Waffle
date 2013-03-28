namespace CommandProcessing
{
    public static class CommandProcessorExtensions
    {
        public static void Process<TCommand>(this ICommandProcessor processor, TCommand command) where TCommand : ICommand
        {
            processor.Process<TCommand, EmptyResult>(command, null);
        }
        
        public static TResult Process<TCommand, TResult>(this ICommandProcessor processor, TCommand command) where TCommand : ICommand
        {
            return processor.Process<TCommand, TResult>(command, null);
        }
    }
}
