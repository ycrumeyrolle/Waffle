namespace CommandProcessing.Filters
{
    public class HandlerExecutingContext
    {
        private HandlerResult result;

        public HandlerExecutingContext(HandlerContext context)
        {
            this.CommandContext = context;
        }

        /// <summary>
        /// Gets or sets the result returned by the action method.
        /// </summary>
        /// <returns>
        /// The result returned by the action method.
        /// </returns>
        /// <value>
        /// The result.
        /// </value>
        public HandlerResult Result
        {
            get
            {
                return this.result ?? EmptyResult.Instance;
            }

            set
            {
                this.result = value;
            }
        }

        public HandlerContext CommandContext { get; set; }
    }
}