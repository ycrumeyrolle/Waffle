namespace Waffle.Commands
{
    using System;
    using System.Collections.Generic;
    using Waffle.Internal;
    
    /// <summary>
    /// Provides an implementation of <see cref="ICommandHandlerTypeResolver"/> with no external dependencies.
    /// </summary>
    public class DefaultCommandHandlerTypeResolver : HandlerTypeResolver, ICommandHandlerTypeResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandHandlerTypeResolver"/> class with a default
        /// filter for detecting handler types.
        /// </summary>
        public DefaultCommandHandlerTypeResolver()
            : base(DefaultCommandHandlerTypeResolver.IsHandlerType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandHandlerTypeResolver"/> class using a predicate to filter handler types.
        /// </summary>
        /// <param name="predicate">
        /// The predicate.
        /// </param>
        public DefaultCommandHandlerTypeResolver(Predicate<Type> predicate)
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
        public ICollection<Type> GetCommandHandlerTypes(IAssembliesResolver assembliesResolver)
        {
            if (assembliesResolver == null)
            {
                throw Error.ArgumentNull("assembliesResolver");
            }

            return this.GetHandlerTypes(assembliesResolver);
        }

        private static bool IsHandlerType(Type t)
        {
            return t != null && t.IsClass && t.IsPublic && !t.IsAbstract && TypeHelper.CommandHandlerType.IsAssignableFrom(t);
        }
    }
}