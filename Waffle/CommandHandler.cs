namespace Waffle
{
    using Waffle.Commands;
    using Waffle.Filters;

    /// <summary>
    /// Represents a base implementation of the command handler and the event handler. 
    /// </summary>
    public abstract class CommandHandler : ICommandHandler
    {
        /// <summary>
        /// Gets or sets the current <see cref="CommandHandlerContext"/>
        /// </summary>
        public CommandHandlerContext CommandContext { get; set; }
    }
}