namespace Waffle
{
    using System.Security.Principal;
    using System.Threading;
    using Waffle.Dependencies;

    /// <summary>
    /// Provides an implementation of the <see cref="IPrincipalProvider"/>.
    /// for providing the current <see cref="IPrincipal"/>.
    /// A different implementation can be registered via the <see cref="IDependencyResolver"/>. 
    /// </summary>
    public class DefaultPrincipalProvider : IPrincipalProvider
    {
        /// <summary>
        /// Gets or sets the current <see cref="IPrincipal"/> from the <see cref="Thread"/>. 
        /// </summary>
        /// <value>The current <see cref="IPrincipal"/>.</value>
        public IPrincipal Principal
        {
            get
            {
                return Thread.CurrentPrincipal;
            }

            set
            {
                Thread.CurrentPrincipal = value;
            }
        }
    }
}
