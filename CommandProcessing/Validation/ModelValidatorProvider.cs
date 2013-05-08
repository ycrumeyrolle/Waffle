namespace CommandProcessing.Validation
{
    using System.Collections.Generic;
    using CommandProcessing.Metadata;

    /// <summary>
    /// Provides a list of validators for a model.
    /// </summary>
    public abstract class ModelValidatorProvider
    {
        /// <summary>
        /// Gets a list of validators associated with this ModelValidatorProvider.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="validatorProviders">The validator providers.</param>
        /// <returns>The list of validators.</returns>
        public abstract IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders);
    }
}
