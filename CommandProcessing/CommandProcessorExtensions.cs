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
        /// <returns>The result of the command.</returns>
        public static void Process<TCommand>(this ICommandProcessor processor, TCommand command) where TCommand : ICommand
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            processor.Process<TCommand, VoidResult>(command);
        }
    }
}
