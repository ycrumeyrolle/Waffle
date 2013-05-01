namespace CommandProcessing.Validation
{
    using System;
    using System.Collections.ObjectModel;

    public class ModelErrorCollection : Collection<ModelError>
    {
        public void Add(Exception exception)
        {
            this.Add(new ModelError(exception));
        }

        public void Add(string errorMessage)
        {
            this.Add(new ModelError(errorMessage));
        }
    }
}
