namespace Waffle.Tracing
{
    using System.Diagnostics.Contracts;
    using Waffle.Filters;

    /// <summary>
    /// General purpose utilities to format strings used in tracing.
    /// </summary>
    internal static class FormattingUtilities
    {
        public static readonly string NullMessage = "null";
        
        public static string HandlerDescriptorToString(HandlerDescriptor descriptor)
        {
            Contract.Assert(descriptor != null);

            return descriptor.Name;
        }
    }
}