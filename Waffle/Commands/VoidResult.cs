namespace Waffle.Commands
{
    /// <summary>
    /// Represents a result that does nothing, such as an handler method that returns nothing.
    /// </summary>
    public sealed class VoidResult
    {
        private static readonly VoidResult Current = new VoidResult();

        private VoidResult()
        {
        }

        /// <summary>
        /// Gets the <see cref="VoidResult"/> instance. 
        /// </summary>
        /// <value>The <see cref="VoidResult"/> instance. </value>
        public static VoidResult Instance
        {
            get
            {
                return VoidResult.Current;
            }
        }
    }
}