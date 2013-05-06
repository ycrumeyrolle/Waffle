namespace CommandProcessing.Validation
{
    public class ModelState
    {
        private readonly ModelErrorCollection errors = new ModelErrorCollection();

        public ModelErrorCollection Errors
        {
            get { return this.errors; }
        }
    }
}
