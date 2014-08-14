namespace Waffle.Events
{
    /// <summary>
    /// Defines the methods that are required for an <see cref="IEventHandler{TEvent}"/> factory.
    /// </summary>
    public interface IEventHandlerSelector
    {
        /// <summary>
        /// Selects the <see cref="EventHandlerDescriptor"/>s for the given <see cref="HandlerRequest"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The collection of <see cref="EventHandlerDescriptor"/>s.</returns>
        EventHandlersDescriptor SelectHandlers(EventHandlerRequest request);
    }
}