namespace CommandProcessing
{
    internal class CommandProcessorWrapper : ICommandProcessor
    {
        private readonly CommandProcessor inner;

        private readonly HandlerRequest request;

        public CommandProcessorWrapper(CommandProcessor inner, HandlerRequest request)
        {
            this.inner = inner;
            this.request = request;
        }

        public ProcessorConfiguration Configuration
        {
            get { return this.inner.Configuration; }
        }

        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <typeparam name="TCommand">The type of command to process.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="command">The command to process.</param>
        /// <returns>The result of the command.</returns>
        public TResult Process<TCommand, TResult>(TCommand command) where TCommand : ICommand
        {
            return this.inner.Process<TCommand, TResult>(command, this.request);
        }

        public TService Using<TService>() where TService : class
        {
            return this.inner.Using<TService>();
        }
    }
}
