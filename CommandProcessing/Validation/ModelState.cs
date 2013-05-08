namespace CommandProcessing.Validation
{
    /// <summary>
    /// Encapsulates the state of model validation.
    /// </summary>
    public class ModelState
    {
        private readonly ModelErrorCollection errors = new ModelErrorCollection();

        /// <summary>
        /// Gets a <see cref="ModelErrorCollection"/> object that contains any errors that occurred during model validation.
        /// </summary>
        /// <value>The errors.</value>
        public ModelErrorCollection Errors
        {
            get { return this.errors; }
        }
    }
}
