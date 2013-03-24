namespace CommandProcessing
{
    /// <summary>
    /// Represents a result that does nothing, such as a handler action method that returns nothing.
    /// </summary>
    public class EmptyResult
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