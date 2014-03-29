namespace Waffle.Filters
{
    /// <summary>
    /// Defines the lifetime of an handler.
    /// </summary>
    public enum HandlerLifetime
    {
        /// <summary>
        /// The handler will be always instanciated.
        /// </summary>
        Transient,

        /// <summary>
        /// The handler will be instanciated only once per request. Subrequest will have the same instance.
        /// </summary>
        PerRequest,

        /// <summary>
        /// The handler will be instanciated only once.
        /// </summary>
        Singleton
    }
}
