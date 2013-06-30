namespace Waffle
{
    using System;
    using Microsoft.Practices.Unity;
    using Waffle.Unity;

    /// <summary>
    /// This provides an easy way to handle Unity with the <see cref="CommandProcessor"/>.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Registers the Unity <see cref="DependencyResolver"/> as dependency resolver for the <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="container">The Unity container to register.</param>
        public static DependencyResolver RegisterContainer(this ProcessorConfiguration configuration, IUnityContainer container)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            DependencyResolver dependencyResolver = new DependencyResolver(container);
            configuration.DependencyResolver = dependencyResolver;
            dependencyResolver.RegisterHandlers(configuration);
            return dependencyResolver;
        }
    }
}
