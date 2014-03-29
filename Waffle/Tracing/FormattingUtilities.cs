namespace Waffle.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using Waffle.Events;
    using Waffle.Filters;

    /// <summary>
    /// General purpose utilities to format strings used in tracing.
    /// </summary>
    internal static class FormattingUtilities
    {
        public const string NullMessage = "null";

        public static string HandlerDescriptorToString(HandlerDescriptor descriptor)
        {
            Contract.Requires(descriptor != null);

            return descriptor.Name;
        }
        
        public static string EventHandlerDescriptorsToString(IEnumerable<EventHandlerDescriptor> descriptors)
        {
            Contract.Requires(descriptors != null);

            return string.Join(", ", descriptors.Select(HandlerDescriptorToString));
        }

        public static string ValueToString(object value, CultureInfo cultureInfo)
        {
            Contract.Requires(cultureInfo != null);

            if (value == null)
            {
                return NullMessage;
            }

            return Convert.ToString(value, cultureInfo);
        }
    }
}