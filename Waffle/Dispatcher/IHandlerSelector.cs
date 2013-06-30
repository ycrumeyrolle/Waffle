namespace Waffle.Dispatcher
{
    using Waffle.Filters;

    /// <summary>
    /// Defines the methods that are required for an <see cref="Handler"/> factory.
    /// </summary>
    public interface IHandlerSelector
    {
        /// <summary>
        /// Selects a <see cref="HandlerDescriptor"/> for the given <see cref="HandlerRequest"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>An <see cref="HandlerDescriptor"/> instance.</returns>
        HandlerDescriptor SelectHandler(HandlerRequest request);
    }
}