namespace CommandProcessing.Dispatcher
{
    using CommandProcessing.Filters;

    /// <summary>
    /// Provides an abstraction for getting the name of an handler. 
    /// </summary>
    public interface IHandlerNameResolver
    { 
        /// <summary>
        /// Returns the name of the handler.
        /// </summary>
        /// <param name="descriptor">
        /// The <see cref="HandlerDescriptor"/>.
        /// </param>
        /// <returns>
        /// The name of the handler.
        /// </returns>
        string GetHandlerName(HandlerDescriptor descriptor);
    }
}