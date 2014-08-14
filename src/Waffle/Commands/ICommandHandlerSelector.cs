namespace Waffle.Commands
{
    /// <summary>
    /// Defines the methods that are required for an <see cref="CommandHandler"/> factory.
    /// </summary>
    public interface ICommandHandlerSelector
    {
        /// <summary>
        /// Selects a <see cref="CommandHandlerDescriptor"/> for the given <see cref="HandlerRequest"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>An <see cref="CommandHandlerDescriptor"/> instance.</returns>
        CommandHandlerDescriptor SelectHandler(CommandHandlerRequest request);
    }
}