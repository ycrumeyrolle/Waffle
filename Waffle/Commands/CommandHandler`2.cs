namespace Waffle.Commands
{
    using Waffle.Filters;

    /// <summary>
    /// Represents the command handler. 
    /// It is responsible to do the real processing.
    /// </summary>
    /// <remarks>
    /// This override is a resulting handler.
    /// </remarks>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public abstract class CommandHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult> where TCommand : ICommand
    {
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <param name="context">The <see cref="CommandHandlerContext"/>.</param>
        /// <returns>The result object.</returns>
        public abstract TResult Handle(TCommand command, CommandHandlerContext context);

        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <param name="context">The <see cref="CommandHandlerContext"/>.</param>
        /// <returns>The result object.</returns>
        object ICommandHandler.Handle(ICommand command, CommandHandlerContext context)
        {
            if (context.Descriptor.ResultType == null || context.Descriptor.ResultType == typeof(void))
            {
                context.Descriptor.HandleVoidMethod(this, command, context);
                return null;
            }

            var result = context.Descriptor.HandleMethod(this, command, context);
            return result;
        }
    }
}