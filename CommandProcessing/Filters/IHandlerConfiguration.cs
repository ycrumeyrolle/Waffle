namespace CommandProcessing.Filters
{
    /// <summary>
    /// If a handler is decorated with an attribute with this interface, then it gets invoked to initialize the handler settings. 
    /// </summary>
    public interface IHandlerConfiguration
    {
        /// <summary>
        /// Callback invoked to set per-handler overrides for this <paramref name="descriptor"/>. 
        /// </summary>
        /// <param name="settings">The handler settings to initialize.</param>
        /// <param name="descriptor">The handler descriptor. Note that the <see cref="HandlerDescriptor"/> can be associated with the derived handler type given that IHandlerConfiguration is inherited.</param>
        void Initialize(HandlerSettings settings, HandlerDescriptor descriptor);
    }
}
