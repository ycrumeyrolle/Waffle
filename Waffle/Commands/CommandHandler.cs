namespace Waffle.Commands
{
    using Waffle.Filters;
    using Waffle.Internal;

    /// <summary>
    /// Represents the command handler. 
    /// It is responsible to do the real processing.
    /// </summary>
    public abstract class CommandHandler : ICommandHandler
    {
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <param name="context">The <see cref="CommandHandlerContext"/>.</param>
        /// <returns>The result object.</returns>
        public virtual object Handle(ICommand command, CommandHandlerContext context)
        {
            if (command == null)
            {
                throw Error.ArgumentNull("command");
            }

            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }
            
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