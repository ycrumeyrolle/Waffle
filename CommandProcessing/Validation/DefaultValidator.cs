namespace CommandProcessing.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using CommandProcessing.Internal;

    /// <summary>
    /// Represents a class used to recursively validate a command.
    /// </summary>
    /// <remarks>Use internaly the <see cref="System.ComponentModel.DataAnnotations.Validator"/>.</remarks>
    public class DefaultCommandValidator : ICommandValidator
    {
        /// <summary>
        /// Determines whether the command is valid and adds any validation errors to the command's ValidationResults.
        /// </summary>
        /// <param name="command">The command to be validated.</param>
        /// <returns>true if command is valid, false otherwise.</returns>
        public bool Validate(ICommand command)
        {
            if (command == null)
            {
                throw Error.ArgumentNull("command");
            } 

            ValidationContext context = new ValidationContext(command, null, null);
            bool valid = Validator.TryValidateObject(command, context, command.ValidationResults, true);
            
            return valid;
        }
    }
}
