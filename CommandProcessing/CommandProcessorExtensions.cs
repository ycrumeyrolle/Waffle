namespace CommandProcessing
{
    using System;
    using CommandProcessing.Internal;

    public static class CommandProcessorExtensions
    {
        public static void Process<TCommand>(this ICommandProcessor processor, TCommand command, HandlerRequest currentRequest) where TCommand : ICommand
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            processor.Process<TCommand, EmptyResult>(command, currentRequest);
        }

        public static void Process<TCommand>(this ICommandProcessor processor, TCommand command) where TCommand : ICommand
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            processor.Process<TCommand, EmptyResult>(command, null);
        }
        
        public static TResult Process<TCommand, TResult>(this ICommandProcessor processor, TCommand command) where TCommand : ICommand
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            return processor.Process<TCommand, TResult>(command, null);
        }
    }
}
