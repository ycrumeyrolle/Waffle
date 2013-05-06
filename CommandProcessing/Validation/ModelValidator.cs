namespace CommandProcessing.Validation
{
    using System.Collections.Generic;
    using CommandProcessing.Internal;
    using CommandProcessing.Metadata;

    public abstract class ModelValidator
    {
        protected ModelValidator(IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            if (validatorProviders == null)
            {
                throw Error.ArgumentNull("validatorProviders");
            }

            this.ValidatorProviders = validatorProviders;
        }

        protected internal IEnumerable<ModelValidatorProvider> ValidatorProviders { get; private set; }

        public abstract IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container);
    }
}
