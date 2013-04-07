namespace CommandProcessing
{
    /// <summary>
    /// Represents a result that does nothing, such as an handler method that returns nothing.
    /// </summary>
    public sealed class EmptyResult
    {
        private static readonly EmptyResult Current = new EmptyResult();

        private EmptyResult()
        {
        }

        internal static EmptyResult Instance
        {
            get
            {
                return EmptyResult.Current;
            }
        }
    }
}