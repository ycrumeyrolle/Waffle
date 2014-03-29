namespace Waffle
{
    using System;
    using Waffle.Filters;

    /// <summary>
    /// Represents an attribute that allow to define the lifetime of an handler as a singleton.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SingletonHandlerAttribute : Attribute, IHandlerLifetimeProvider
    {
        /// <summary>
        /// Gets the handler lifetime.
        /// </summary>
        /// <value><see cref="Waffle.Filters.HandlerLifetime.Singleton"/>.</value>
        public HandlerLifetime HandlerLifetime
        {
            get
            {
                return HandlerLifetime.Singleton;
            }
        }
    }
}