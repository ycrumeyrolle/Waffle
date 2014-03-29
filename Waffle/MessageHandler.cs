namespace Waffle
{
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Filters;

    /// <summary>
    /// Represents a base implementation of the command handler and the event handler. 
    /// </summary>
    public abstract class MessageHandler : ICommandHandler, IEventHandler
    {
        public CommandHandlerContext CommandContext { get; set; }

        public EventHandlerContext EventContext { get; set; }
    }
}