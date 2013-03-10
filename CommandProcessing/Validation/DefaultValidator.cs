namespace CommandProcessing.Validation
{
    using System.ComponentModel.DataAnnotations;

    public class DefaultCommandValidator : ICommandValidator
    {
        public bool Validate(ICommand command)
        {
            ValidationContext context = new ValidationContext(command, null, null);
            bool valid = Validator.TryValidateObject(command, context, command.ValidationResults, true);
            
            return valid;
        }
    }
}
