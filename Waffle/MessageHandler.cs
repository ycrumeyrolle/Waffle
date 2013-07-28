namespace Waffle
{
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Filters;
    using Waffle.Internal;

    public abstract class MessageHandler : ICommandHandler, IEventHandler
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

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        /// <param name="context">The <see cref="EventHandlerContext"/>.</param>
        void IEventHandler.Handle(IEvent @event, EventHandlerContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            context.Descriptor.HandleMethod(this, @event, context);
        }
    }
}