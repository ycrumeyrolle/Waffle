namespace Waffle.Commands
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a result that does nothing, such as an handler method that returns nothing.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct VoidResult
    {
    }
}