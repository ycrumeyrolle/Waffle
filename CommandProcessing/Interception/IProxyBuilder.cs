namespace CommandProcessing.Interception
{
    /// <summary>
    /// Defines the methods that are required to build a proxy.
    /// </summary>
    public interface IProxyBuilder
    {
        /// <summary>
        /// Build a proxy from a source object.
        /// </summary>
        /// <typeparam name="T">The type of the proxy to build.</typeparam>
        /// <param name="source">The object who will be proxied.</param>
        /// <param name="interceptorProvider">The interceptor provider.</param>
        /// <returns>A proxy of the source object. Only the virtual method/properties will be proxied.</returns>
        T Build<T>(T source, IInterceptionProvider interceptorProvider) where T : class;
    }
}