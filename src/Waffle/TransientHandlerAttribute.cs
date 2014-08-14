namespace Waffle
{
    using System;
    using Waffle.Filters;

    /// <summary>
    /// Represents an attribute that allow to define the lifetime of an handler as transient.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class TransientHandlerAttribute : Attribute, IHandlerLifetimeProvider
    {
        /// <summary>
        /// Gets the handler lifetime.
        /// </summary>
        /// <value><see cref="Waffle.Filters.HandlerLifetime.Transient"/>.</value>
        public HandlerLifetime HandlerLifetime
        {
            get
            {
                return HandlerLifetime.Transient;
            }
        }
    }
}