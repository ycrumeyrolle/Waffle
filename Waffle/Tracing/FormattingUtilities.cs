namespace Waffle.Tracing
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Waffle.Events;
    using Waffle.Filters;

    /// <summary>
    /// General purpose utilities to format strings used in tracing.
    /// </summary>
    internal static class FormattingUtilities
    {
        public static string HandlerDescriptorToString(HandlerDescriptor descriptor)
        {
            Contract.Assert(descriptor != null);

            return descriptor.Name;
        }
        
        public static string EventHandlerDescriptorsToString(IEnumerable<EventHandlerDescriptor> descriptors)
        {
            Contract.Assert(descriptors != null);

            return string.Join(", ", descriptors.Select(HandlerDescriptorToString));
        }
    }
}