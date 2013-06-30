namespace Waffle.Validation
{
    using System;
    using Waffle.Internal;

    /// <summary>
    /// Represents an error that occurs during model validation.
    /// </summary>
    public class ModelError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelError"/> class by using the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public ModelError(Exception exception)
            : this(exception, errorMessage: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelError"/> class by using the specified exception and error message.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="errorMessage">The error message.</param>
        public ModelError(Exception exception, string errorMessage)
            : this(errorMessage)
        {
            if (exception == null)
            {
                throw Error.ArgumentNull("exception");
            }

            this.Exception = exception;
        }

        /// <summary>
        /// Initializes a new instance of the  <see cref="ModelError"/> class by using the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public ModelError(string errorMessage)
        {
            this.ErrorMessage = errorMessage ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets the exception object.
        /// </summary>
        /// <value>The <see cref="Exception"/>.</value>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage { get; private set; }
    }
}
