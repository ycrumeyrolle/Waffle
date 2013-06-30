namespace Waffle
{
    using System.Security.Principal;
    using Waffle.Dependencies;

    /// <summary>
    /// Provides an abstraction for providing the current <see cref="IPrincipal"/>.
    /// A different implementation can be registered via the <see cref="IDependencyResolver"/>. 
    /// </summary>
    public interface IPrincipalProvider
    {
        /// <summary>
        /// Gets or sets the current <see cref="IPrincipal"/>. 
        /// </summary>
        /// <value>The current <see cref="IPrincipal"/>.</value>
        IPrincipal Principal { get; set; }
    }
}