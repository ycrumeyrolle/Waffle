namespace CommandProcessing.Dispatcher
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;

    /// <summary>
    /// Provides an implementation of <see cref="IHandlerNameResolver"/> with no external dependencies.
    /// </summary>
    public class DefaultHandlerNameResolver : IHandlerNameResolver
    {
        /// <summary>
        /// Returns the name of the handler.
        /// </summary>
        /// <remarks>
        /// The default behavior is to look for the <see cref="DisplayAttribute"/> on the handler or the handling method.
        /// </remarks>
        /// <param name="descriptor">
        /// The <see cref="HandlerDescriptor"/>.
        /// </param>
        /// <returns>
        /// The name of the handler.
        /// </returns>
        public string GetHandlerName(HandlerDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw Error.ArgumentNull("descriptor");
            }

            DisplayNameAttribute displayNameAttribute = descriptor.GetCustomAttributes<DisplayNameAttribute>().FirstOrDefault();
            if (displayNameAttribute != null)
            {
                return displayNameAttribute.DisplayName;
            }

            return descriptor.HandlerType.Name;
        }
    }
}