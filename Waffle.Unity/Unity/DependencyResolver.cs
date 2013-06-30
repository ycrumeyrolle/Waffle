namespace Waffle.Unity
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Practices.Unity;
    using Waffle.Dependencies;

    /// <summary>
    /// Represents a dependency injection container with Unity.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "False positive. IDisposable is not réimplemented and Dispose method should not be overriden.")]
    public sealed class DependencyResolver : DependencyScope, IDependencyResolver
    {    
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyResolver"/> class. 
        /// </summary>
        public DependencyResolver(IUnityContainer container)
            : base(container)
        {
        }

        /// <summary>
        /// Starts a resolution scope. Objects which are resolved in the given scope will belong to
        /// that scope, and when the scope is disposed, those objects are returned to the container.
        /// Returns a new instance of <see cref="IDependencyScope"/> every time this
        /// method is called.
        /// </summary>
        /// <returns>The dependency scope.</returns>
        public IDependencyScope BeginScope()
        {
            IUnityContainer child = this.Container.CreateChildContainer();
            return DependencyScope.CreateScope(child);
        }
    }
}