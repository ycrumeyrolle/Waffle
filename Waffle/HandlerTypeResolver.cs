namespace Waffle
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Waffle.Internal;

    /// <summary>
    /// Represents a resolver of types according a predicate.
    /// </summary>
    public class HandlerTypeResolver 
    {
        private readonly Predicate<Type> isHandlerTypePredicate;
     
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerTypeResolver"/> class using a predicate to filter handler types.
        /// </summary>
        /// <param name="predicate">
        /// The predicate.
        /// </param>
        public HandlerTypeResolver(Predicate<Type> predicate)
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
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We deliberately ignore all exceptions when building the cache.")]
        public ICollection<Type> GetHandlerTypes(IAssembliesResolver assembliesResolver)
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
                    exportedTypes = assembly.GetTypes();
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
                    result.AddRange(exportedTypes.Where(type => TypeIsVisible(type) && this.isHandlerTypePredicate(type)));
                }
            }

            return result;
        } 
       
           private static bool TypeIsVisible(Type type)   
           {   
               return (type != null && type.IsVisible);   
           }  

    }
}