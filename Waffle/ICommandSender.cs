namespace Waffle
{
    /// <summary>
    /// Represents a command sender. 
    /// </summary>
    public interface ICommandSender
    {
        /// <summary>
        /// Gets the processor in charge to send commands.
        /// </summary>
        /// <value>The <see cref="IMessageProcessor"/>.</value>
        IMessageProcessor Processor { get; }
    }
}
