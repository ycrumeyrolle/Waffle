namespace CommandProcessing
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public interface ICommand
    {
        bool IsValid { get; }

        ICollection<ValidationResult> ValidationResults { get; }
    }
}