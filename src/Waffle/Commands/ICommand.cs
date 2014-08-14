namespace Waffle.Commands
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a command to transmit to the processor.
    /// The implementations contain all the properties that will be validated and provided to the handlers.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "This interface intend to be a marker interface.")]
    public interface ICommand 
    {
    }
}