namespace CommandProcessing.Validation
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents a collection of <see cref="ModelError"/> instances.
    /// </summary>
    public class ModelErrorCollection : Collection<ModelError>
    {
        /// <summary>
        /// Adds the specified <see cref="Exception"/> object to the model-error collection.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void Add(Exception exception)
        {
            this.Add(new ModelError(exception));
        }

        /// <summary>
        /// Adds the specified error message to the model-error collection.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public void Add(string errorMessage)
        {
            this.Add(new ModelError(errorMessage));
        }
    }
}
