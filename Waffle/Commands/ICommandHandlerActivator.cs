namespace Waffle.Commands
{
    using Waffle.Filters;

    /// <summary>
    /// Defines the methods that are required to create the <see cref="CommandHandler"/>.
    /// </summary>
    public interface ICommandHandlerActivator
    {
        /// <summary>
        /// Creates the <see cref="CommandHandler"/> specified by <paramref name="descriptor"/> using the given <paramref name="request"/>.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="descriptor">
        /// The handler descriptor.
        /// </param>
        /// <returns>
        /// The <see cref="CommandHandler"/>.
        /// </returns>
        ICommandHandler Create(HandlerRequest request, HandlerDescriptor descriptor);
    }
}