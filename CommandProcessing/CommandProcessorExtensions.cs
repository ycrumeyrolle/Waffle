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
        /// <param name="processor">The command processor.</param>
        /// <param name="command">The command to process.</param>
        /// <returns>The result of the command.</returns>
        public static void Process(this ICommandProcessor processor, ICommand command)
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            processor.Process<VoidResult>(command);
        }
    }
}
