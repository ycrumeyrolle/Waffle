namespace CommandProcessing.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CommandProcessing.Internal;

    /// <summary>
    /// Provides an implementation of <see cref="IHandlerTypeResolver"/> with no external dependencies.
    /// </summary>
    public class DefaultHandlerTypeResolver : IHandlerTypeResolver
    {
        private readonly Predicate<Type> isHandlerTypePredicate;
     
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHandlerTypeResolver"/> class with a default
        /// filter for detecting handler types.
        /// </summary>
        public DefaultHandlerTypeResolver()
            : this(DefaultHandlerTypeResolver.IsHandlerType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHandlerTypeResolver"/> class using a predicate to filter controller types.
        /// </summary>
        /// <param name="predicate">
        /// The predicate.
        /// </param>
        public DefaultHandlerTypeResolver(Predicate<Type> predicate)
        {
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }

            this.isHandlerTypePredicate = predicate;
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
        public virtual ICollection<Type> GetHandlerTypes(IAssembliesResolver assembliesResolver)
        {
            if (assembliesResolver == null)
            {
                throw Error.ArgumentNull("assembliesResolver");
            }

            List<Type> result = new List<Type>();

            // Go through all assemblies referenced by the application and search for types matching a predicate
            ICollection<Assembly> assemblies = assembliesResolver.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type[] exportedTypes;
                if (assembly == null || assembly.IsDynamic)
                {
                    // can't call GetExportedTypes on a dynamic assembly
                    continue;
                }

                try
                {
                    exportedTypes = assembly.GetExportedTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    exportedTypes = ex.Types;
                }
                catch
                {
                    // We deliberately ignore all exceptions when building the cache.
                    continue;
                }

                if (exportedTypes != null)
                {
                    result.AddRange(exportedTypes.Where(type => this.isHandlerTypePredicate(type)));
                }
            }

            return result;
        }

        private static bool IsHandlerType(Type t)
        {
            return t != null && t.IsClass && t.IsPublic && !t.IsAbstract && TypeHelper.HandlerType.IsAssignableFrom(t);
        }
    }
}