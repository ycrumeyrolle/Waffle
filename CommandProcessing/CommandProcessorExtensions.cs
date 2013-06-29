namespace CommandProcessing
{
    using System.Threading.Tasks;
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
        /// <returns>The <see cref="Task"/> returning the result of the command.</returns>
        public static Task ProcessAsync(this ICommandProcessor processor, ICommand command)
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            return processor.ProcessAsync<VoidResult>(command);
        }

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

            Task task = processor.ProcessAsync(command);
            task.Wait();
        }

        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <param name="processor">The command processor.</param>
        /// <param name="command">The command to process.</param>
        /// <returns>The result of the command.</returns>
        public static TResult Process<TResult>(this ICommandProcessor processor, ICommand command)
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            Task<TResult> task = processor.ProcessAsync<TResult>(command);
            return task.Result;
        }
    }
}
