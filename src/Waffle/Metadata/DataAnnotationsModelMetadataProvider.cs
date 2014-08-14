namespace Waffle.Metadata
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Implements the default model metadata provider.
    /// </summary>
    public class DataAnnotationsModelMetadataProvider : AssociatedMetadataProvider<CachedDataAnnotationsModelMetadata>
    {
        /// <summary>
        /// Creates the metadata for the specified property.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="containerType">The type of the container.</param>
        /// <param name="modelType">The type of the model.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The metadata for the property.</returns>
        protected override CachedDataAnnotationsModelMetadata CreateMetadataPrototype(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName)
        {
            return new CachedDataAnnotationsModelMetadata(this, containerType, modelType, propertyName, attributes);
        }

        /// <summary>
        /// Creates the metadata from prototype for the specified property.
        /// </summary>
        /// <param name="prototype">The prototype.</param>
        /// <param name="modelAccessor">The model accessor.</param>
        /// <returns>The metadata for the property.</returns>
        protected override CachedDataAnnotationsModelMetadata CreateMetadataFromPrototype(CachedDataAnnotationsModelMetadata prototype, Func<object> modelAccessor)
        {
            return new CachedDataAnnotationsModelMetadata(prototype, modelAccessor);
        }
    }
}