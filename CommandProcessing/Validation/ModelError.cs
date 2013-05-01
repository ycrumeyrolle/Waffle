namespace CommandProcessing.Validation
{
    using System;
    using CommandProcessing.Internal;

    public class ModelError
    {
        public ModelError(Exception exception)
            : this(exception, errorMessage: null)
        {
        }

        public ModelError(Exception exception, string errorMessage)
            : this(errorMessage)
        {
            if (exception == null)
            {
                throw Error.ArgumentNull("exception");
            }

            this.Exception = exception;
        }

        public ModelError(string errorMessage)
        {
            this.ErrorMessage = errorMessage ?? string.Empty;
        }

        public Exception Exception { get; private set; }

        public string ErrorMessage { get; private set; }
    }
}
