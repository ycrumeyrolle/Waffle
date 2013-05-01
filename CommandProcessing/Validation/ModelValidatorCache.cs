namespace CommandProcessing.Validation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using CommandProcessing.Metadata;

    /// <summary>
    /// Defines a cache for <see cref="ModelValidator"/>s. This cache is keyed on the type or property that the metadata is associated with.
    /// </summary>
    internal class ModelValidatorCache : IModelValidatorCache
    {
        private readonly ConcurrentDictionary<Tuple<Type, string>, ModelValidator[]> validatorCache = new ConcurrentDictionary<Tuple<Type, string>, ModelValidator[]>();
        private readonly Lazy<IEnumerable<ModelValidatorProvider>> validatorProviders;

        public ModelValidatorCache(Lazy<IEnumerable<ModelValidatorProvider>> validatorProviders)
        {
            this.validatorProviders = validatorProviders;
        }

        public IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata)
        {
            // If metadata is for a property then containerType != null && propertyName != null
            // If metadata is for a type then containerType == null && propertyName == null, so we have to use modelType for the cache key.
            Type typeForCache = metadata.ContainerType ?? metadata.ModelType;
            Tuple<Type, string> cacheKey = Tuple.Create(typeForCache, metadata.PropertyName);

            ModelValidator[] validators;
            if (!this.validatorCache.TryGetValue(cacheKey, out validators))
            {
                // Compute validators
                // There are no side-effects if the same validators are created more than once
                validators = metadata.GetValidators(this.validatorProviders.Value).ToArray();
                this.validatorCache.TryAdd(cacheKey, validators);
            }
            return validators;
        }
    }
}
