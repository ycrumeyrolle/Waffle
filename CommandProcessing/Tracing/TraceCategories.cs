namespace CommandProcessing.Tracing
{
    /// <summary>
    /// Category names traced by the default tracing implementation.
    /// </summary>
    /// <remarks>
    /// The list of permitted category names is open-ended, and users may define their own.
    ///     It is recommended that category names reflect the namespace of their
    ///     respective area.  This prevents name conflicts and allows external
    ///     logging tools to enable or disable tracing by namespace.
    /// </remarks>
    public static class TraceCategories
    {
        /// <summary>
        /// The handlers category.
        /// </summary>
        public static readonly string HandlersCategory = "CommandProcessing.Handlers";

        /// <summary>
        /// The filters category.
        /// </summary>
        public static readonly string FiltersCategory = "CommandProcessing.Filters";

        /// <summary>
        /// The request category.
        /// </summary>
        public static readonly string RequestsCategory = "CommandProcessing.Requests";
    }
}