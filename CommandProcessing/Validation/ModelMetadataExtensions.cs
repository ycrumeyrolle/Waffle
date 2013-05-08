namespace CommandProcessing.Validation
{
    using System.Collections.Generic;
    using System.Linq;
    using CommandProcessing.Internal;
    using CommandProcessing.Metadata;

    /// <summary>
    /// Provides extensions methods for the <see cref="ModelMetadata"/> class.
    /// </summary>
    public static class ModelMetadataExtensions
    {  
        /// <summary>
        /// Gets a list of validators associated with this ModelValidatorProvider.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="validatorProviders">The validator providers.</param>
        /// <returns>The list of validators.</returns>
        /// </summary>
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
