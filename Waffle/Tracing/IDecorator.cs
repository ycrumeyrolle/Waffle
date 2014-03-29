namespace Waffle.Tracing
{
    /// <summary>
    /// The Decorator interface.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public interface IDecorator<out T>
    {
        /// <summary>
        /// Gets the inner.
        /// </summary>
        /// <value>
        /// The inner.
        /// </value>
        T Inner { get; }
    }
}