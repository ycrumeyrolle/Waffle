namespace Waffle.Commands
{
    using Waffle.Filters;
    using Waffle.Internal;

    /// <summary>
    /// Represents the command handler. 
    /// It is responsible to do the real processing.
    /// </summary>
    /// <remarks>
    /// This override is a result-less handler.
    /// </remarks>
    /// <typeparam name="TCommand">The command type.</typeparam>
    public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand, VoidResult>
        where TCommand : ICommand
    {
        VoidResult ICommandHandler<TCommand, VoidResult>.Handle(TCommand command, CommandHandlerContext context)
        {
            this.Handle(command, context);
            return null;
        }
        
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <param name="context">The <see cref="CommandHandlerContext"/>.</param>
        public abstract void Handle(TCommand command, CommandHandlerContext context);

        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <param name="context">The <see cref="CommandHandlerContext"/>.</param>
        /// <returns>The result object.</returns>
        public object Handle(ICommand command, CommandHandlerContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            context.Descriptor.HandleVoidMethod(this, command, context);
            return null;
        }
    }
}