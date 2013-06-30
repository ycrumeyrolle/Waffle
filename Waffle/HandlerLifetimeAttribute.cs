namespace Waffle
{
    using System;
    using Waffle.Filters;

    /// <summary>
    /// Represents an attribute that allow to define the lifetime of an handler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class HandlerLifetimeAttribute : Attribute
    {   
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerLifetimeAttribute"/> class. 
        /// <param name="handlerLifetime">The handler lifetime.</param>
        /// </summary>
        public HandlerLifetimeAttribute(HandlerLifetime handlerLifetime)
        {
            this.HandlerLifetime = handlerLifetime;
        }

        /// <summary>
        /// Gets or sets the handler lifetime.
        /// </summary>
        /// <value>The handler lifetime..</value>
        public HandlerLifetime HandlerLifetime { get; private set; }
    }
}
