namespace CommandProcessing
{
    using CommandProcessing.Internal;

    /// <summary>
    /// Provides methods extensions to the <see cref="CommandProcessor"/>.
    /// </summary>
    public static class CommandProcessorExtensions
    {
        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <typeparam name="TCommand">The type of command to process.</typeparam>
        /// <param name="processor">The command processor.</param>
        /// <param name="command">The command to process.</param>
        /// <param name="currentRequest">The current request. Pass null if there is not parent request.</param>
        /// <returns>The result of the command.</returns>
        public static void Process<TCommand>(this ICommandProcessor processor, TCommand command, HandlerRequest currentRequest) where TCommand : ICommand
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            processor.Process<TCommand, VoidResult>(command, currentRequest);
        }

        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <typeparam name="TCommand">The type of command to process.</typeparam>
        /// <param name="processor">The command processor.</param>
        /// <param name="command">The command to process.</param>
        /// <returns>The result of the command.</returns>
        public static void Process<TCommand>(this ICommandProcessor processor, TCommand command) where TCommand : ICommand
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            processor.Process<TCommand, VoidResult>(command, null);
        }

        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <typeparam name="TCommand">The type of command to process.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="processor">The command processor.</param>
        /// <param name="command">The command to process.</param>
        /// <returns>The result of the command.</returns>
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
