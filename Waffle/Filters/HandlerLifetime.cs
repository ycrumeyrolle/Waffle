namespace Waffle.Filters
{
    /// <summary>
    /// Defines the lifetime of an handler.
    /// </summary>
    public enum HandlerLifetime
    {
        /// <summary>
        /// The handler will be always newly instanciated.
        /// </summary>
        Transcient,

        /// <summary>
        /// The handler will be instanciated only once per request. Subrequest will have the same instance.
        /// </summary>
        PerRequest,

        /// <summary>
        /// The handler will be instancied only once per processor. 
        /// </summary>
        Processor
    }
}
