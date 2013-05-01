namespace CommandProcessing
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

        public static VoidResult Instance
        {
            get
            {
                return VoidResult.Current;
            }
        }
    }
}