namespace CommandProcessing
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Command : ICommand
    {
        private readonly ICollection<ValidationResult> validation = new List<ValidationResult>();

        public virtual bool IsValid
        {
            get
            {
                return this.ValidationResults.Count == 0;
            }
        }

        public virtual ICollection<ValidationResult> ValidationResults
        {
            get
            {
                return this.validation;
            }
        }
    }
}