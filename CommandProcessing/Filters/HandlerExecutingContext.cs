namespace CommandProcessing.Filters
{
    public class HandlerExecutingContext
    {
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
        public object Result { get; set; }

        public HandlerContext CommandContext { get; set; }
    }
}