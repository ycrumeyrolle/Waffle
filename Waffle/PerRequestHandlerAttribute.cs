namespace Waffle
{
    using System;
    using Waffle.Filters;

    /// <summary>
    /// Represents an attribute that allow to define the lifetime of an handler as a unique per request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PerRequestHandlerAttribute : Attribute, IHandlerLifetimeProvider
    {
        /// <summary>
        /// Gets the handler lifetime.
        /// </summary>
        /// <value><see cref="Waffle.Filters.HandlerLifetime.PerRequest"/>.</value>
        public HandlerLifetime HandlerLifetime
        {
            get
            {
                return HandlerLifetime.PerRequest;
            }
        }
    }
}