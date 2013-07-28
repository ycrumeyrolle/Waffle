namespace Waffle.Events
{
    using System;
    using System.Collections.Generic;
    using Waffle.Internal;

    /// <summary>
    /// Provides an implementation of <see cref="IEventHandlerTypeResolver"/> with no external dependencies.
    /// </summary>
    public class DefaultEventHandlerTypeResolver : HandlerTypeResolver, IEventHandlerTypeResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEventHandlerTypeResolver"/> class with a default
        /// filter for detecting handler types.
        /// </summary>
        public DefaultEventHandlerTypeResolver()
            : base(DefaultEventHandlerTypeResolver.IsHandlerType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEventHandlerTypeResolver"/> class using a predicate to filter handler types.
        /// </summary>
        /// <param name="predicate">
        /// The predicate.
        /// </param>
        public DefaultEventHandlerTypeResolver(Predicate<Type> predicate)
            : base(predicate)
        {
        }

        /// <summary>
        /// Returns a list of handlers available for the application.
        /// </summary>
        /// <param name="assembliesResolver">
        /// The <see cref="IAssembliesResolver"/>.
        /// </param>
        /// <returns>
        /// An <see cref="ICollection{Type}"/> of handlers.
        /// </returns>
        public ICollection<Type> GetEventHandlerTypes(IAssembliesResolver assembliesResolver)
        {
            if (assembliesResolver == null)
            {
                throw Error.ArgumentNull("assembliesResolver");
            }

            return this.GetHandlerTypes(assembliesResolver);
        }

        private static bool IsHandlerType(Type t)
        {
            return t != null && t.IsClass && t.IsPublic && !t.IsAbstract && TypeHelper.EventHandlerType.IsAssignableFrom(t);
        }
    }
}