namespace CommandProcessing.Filters
{
    /// <summary>Represents a result that does nothing, such as a handler action method that returns nothing.</summary>
    public class EmptyResult : HandlerResult
    {
        private static readonly EmptyResult Singleton = new EmptyResult();

        public EmptyResult()
            : base(null)
        {
        }

        internal static EmptyResult Instance
        {
            get
            {
                return EmptyResult.Singleton;
            }
        }
    }
}