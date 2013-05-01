namespace CommandProcessing.Validation
{
    using System.Collections.Generic;
    using CommandProcessing.Metadata;

    /// <summary>
    /// Defines a cache for <see cref="ModelValidator"/>s. This cache is keyed on the type or property that the metadata is associated with.
    /// </summary>
    internal interface IModelValidatorCache
    {
        IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata);
    }
}
