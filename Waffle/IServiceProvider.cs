namespace Waffle
{
    using System.Diagnostics.CodeAnalysis;

    public interface IServiceProvider
    {
        /// <summary>
        /// Supply a service.
        /// The service will be created by the <see cref="IDependencyResolver"/>.
        /// If the ServiceProxyCreationEnabled is <see langword="true"/>, the service will be a proxy.
        /// </summary>
        /// <typeparam name="TService">The type of the service to supply.</typeparam>
        /// <returns>The service.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Using", Justification = "An other term could be found...")]
        TService Use<TService>() where TService : class;
    }
}
