namespace CommandProcessing.Validation
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;
    using CommandProcessing.Metadata;

    internal static class HandlerContextExtensions
    {  
        /// <summary>
        /// Gets the <see cref="ModelMetadataProvider"/> instance for a given <see cref="HandlerContext"/>.
        /// </summary>
        /// <param name="handlerContext">The context.</param>
        /// <returns>An <see cref="ModelMetadataProvider"/> instance.</returns>
        public static ModelMetadataProvider GetMetadataProvider(this HandlerContext handlerContext)
        {
            if (handlerContext == null)
            {
                throw Error.ArgumentNull("handlerContext");
            }

            return handlerContext.Configuration.Services.GetModelMetadataProvider();
        }

        /// <summary>
        /// Gets the collection of registered <see cref="ModelValidatorProvider"/> instances.
        /// </summary>
        /// <param name="handlerContext">The context.</param>
        /// <returns>A collection of <see cref="ModelValidatorProvider"/> instances.</returns>
        public static IEnumerable<ModelValidatorProvider> GetValidatorProviders(this HandlerContext handlerContext)
        {
            if (handlerContext == null)
            {
                throw Error.ArgumentNull("handlerContext");
            }

            return handlerContext.Configuration.Services.GetModelValidatorProviders();
        }

        /// <summary>
        /// Gets the collection of registered <see cref="ModelValidator"/> instances.
        /// </summary>
        /// <param name="handlerContext">The context.</param>
        /// <param name="metadata">The metadata.</param>
        /// <returns>A collection of registered <see cref="ModelValidator"/> instances.</returns>
        public static IEnumerable<ModelValidator> GetValidators(this HandlerContext handlerContext, ModelMetadata metadata)
        {
            if (handlerContext == null)
            {
                throw Error.ArgumentNull("handlerContext");
            }

            IModelValidatorCache validatorCache = handlerContext.GetValidatorCache();
            return handlerContext.GetValidators(metadata, validatorCache);
        }

        internal static IEnumerable<ModelValidator> GetValidators(this HandlerContext handlerContext, ModelMetadata metadata, IModelValidatorCache validatorCache)
        {
            if (validatorCache == null)
            {
                // slow path: there is no validator cache on the configuration
                return metadata.GetValidators(handlerContext.GetValidatorProviders());
            }
            else
            {
                return validatorCache.GetValidators(metadata);
            }
        }

        internal static IModelValidatorCache GetValidatorCache(this HandlerContext handlerContext)
        {
            Contract.Assert(handlerContext != null);

            ProcessorConfiguration configuration = handlerContext.Configuration;
            return configuration.Services.GetModelValidatorCache();
        }
    }
}
