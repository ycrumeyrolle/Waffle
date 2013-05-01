namespace CommandProcessing.Validation
{
    using System.Collections.Generic;
    using CommandProcessing.Metadata;

    public abstract class ModelValidatorProvider
    {
        public abstract IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders);
    }
}
