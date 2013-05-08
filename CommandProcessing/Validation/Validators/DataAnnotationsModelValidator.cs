namespace CommandProcessing.Validation.Validators
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using CommandProcessing.Internal;
    using CommandProcessing.Metadata;
    using CommandProcessing.Validation;

    /// <summary>
    /// Provides a model validator for <see cref="ValidationAttribute"/>.
    /// </summary>
    public class DataAnnotationsModelValidator : ModelValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataAnnotationsModelValidator"/> class.
        /// </summary>
        /// <param name="validatorProviders">The validator providers.</param>
        /// <param name="attribute">The validation attribute for the model.</param>
        public DataAnnotationsModelValidator(IEnumerable<ModelValidatorProvider> validatorProviders, ValidationAttribute attribute)
            : base(validatorProviders)
        {
            if (attribute == null)
            {
                throw Error.ArgumentNull("attribute");
            }

            this.Attribute = attribute;
        }

        /// <summary>
        /// Gets or sets the validation attribute for the model validator.
        /// </summary>
        /// <value>
        /// The validation attribute for the model validator.
        /// </value>
        protected internal ValidationAttribute Attribute { get; private set; }

        /// <summary>
        /// Validates a specified object.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="container">The container.</param>
        /// <returns>A list of validation results.</returns>
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
