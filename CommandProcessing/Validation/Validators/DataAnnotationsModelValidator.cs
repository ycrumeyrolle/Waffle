namespace CommandProcessing.Validation.Validators
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using CommandProcessing.Internal;
    using CommandProcessing.Metadata;
    using CommandProcessing.Validation;

    public class DataAnnotationsModelValidator : ModelValidator
    {
        public DataAnnotationsModelValidator(IEnumerable<ModelValidatorProvider> validatorProviders, ValidationAttribute attribute)
            : base(validatorProviders)
        {
            if (attribute == null)
            {
                throw Error.ArgumentNull("attribute");
            }

            this.Attribute = attribute;
        }

        protected internal ValidationAttribute Attribute { get; private set; }
        
        public override IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container)
        {
            ValidationContext context = new ValidationContext(container ?? metadata.Model, null, null);
            context.DisplayName = metadata.GetDisplayName();

            ValidationResult result = this.Attribute.GetValidationResult(metadata.Model, context);

            if (result != ValidationResult.Success)
            {
                return new[] { new ModelValidationResult { Message = result.ErrorMessage } };
            }

            return new ModelValidationResult[0];
        }
    }
}
