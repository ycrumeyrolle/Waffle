namespace CommandProcessing.Dispatcher
{
    using CommandProcessing.Filters;

    /// <summary>
    /// Defines the methods that are required to create the <see cref="ICommandHandler"/>.
    /// </summary>
    public interface IHandlerActivator
    {
        /// <summary>
        /// Creates the <see cref="ICommandHandler"/> specified by <paramref name="descriptor"/> using the given <paramref name="request"/>.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="descriptor">
        /// The controller descriptor.
        /// </param>
        /// <returns>
        /// The <see cref="ICommandHandler"/>.
        /// </returns>
        ICommandHandler Create(HandlerRequest request, HandlerDescriptor descriptor);
    }
}