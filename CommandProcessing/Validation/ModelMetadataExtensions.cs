namespace CommandProcessing.Validation
{
    using System.Collections.Generic;
    using System.Linq;
    using CommandProcessing.Internal;
    using CommandProcessing.Metadata;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class ModelMetadataExtensions
    {
        public static IEnumerable<ModelValidator> GetValidators(this ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            if (validatorProviders == null)
            {
                throw Error.ArgumentNull("validatorProviders");
            }

            return validatorProviders.SelectMany(provider => provider.GetValidators(metadata, validatorProviders));
        }
    }
}
