namespace Waffle.Dependencies
{
    using System.Collections.Generic;
    using System.Linq;
    using Waffle.Internal;

    /// <summary>
    /// Provides a type-safe implementation of <see cref="IDependencyScope.GetService(System.Type)" /> and <see cref="IDependencyScope.GetServices(System.Type)" />.
    /// </summary>
    public static class DependencyResolverExtensions
    {
        /// <summary>Resolves singly registered services that support arbitrary object creation.</summary>
        /// <returns>The requested service or object.</returns>
        /// <param name="resolver">The dependency resolver instance that this method extends.</param>
        /// <typeparam name="TService">The type of the requested service or object.</typeparam>
        public static TService GetService<TService>(this IDependencyScope resolver)
        {
            if (resolver == null)
            {
                throw Error.ArgumentNull("resolver");
            }

            return (TService)resolver.GetService(typeof(TService));
        }

        /// <summary>Resolves multiply registered services.</summary>
        /// <returns>The requested services.</returns>
        /// <param name="resolver">The dependency resolver instance that this method extends.</param>
        /// <typeparam name="TService">The type of the requested services.</typeparam>
        public static IEnumerable<TService> GetServices<TService>(this IDependencyScope resolver)
        {
            if (resolver == null)
            {
                throw Error.ArgumentNull("resolver");
            }

            return resolver.GetServices(typeof(TService)).Cast<TService>();
        }
    }
}