namespace CommandProcessing.Metadata
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a container for common metadata, for the <see cref="DataAnnotationsModelMetadataProvider"/> class, for a data model.
    /// </summary>
    public class CachedDataAnnotationsModelMetadata : CachedModelMetadata<CachedDataAnnotationsMetadataAttributes>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedDataAnnotationsModelMetadata"/> class.
        /// </summary>
        /// <param name="prototype">The prototype used to initialize the model metadata.</param>
        /// <param name="modelAccessor">The model accessor.</param>
        public CachedDataAnnotationsModelMetadata(CachedDataAnnotationsModelMetadata prototype, Func<object> modelAccessor)
            : base(prototype, modelAccessor)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedDataAnnotationsModelMetadata"/> class.
        /// </summary>
        /// <param name="provider">The metadata provider.</param>
        /// <param name="containerType">The type of the container.</param>
        /// <param name="modelType">The type of the model.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="attributes">The attributes that provides data for the initialization.</param>
        public CachedDataAnnotationsModelMetadata(DataAnnotationsModelMetadataProvider provider, Type containerType, Type modelType, string propertyName, IEnumerable<Attribute> attributes)
            : base(provider, containerType, modelType, propertyName, new CachedDataAnnotationsMetadataAttributes(attributes))
        {
        }
        
        /// <summary>
        /// Retrieves the description of the model.
        /// </summary>
        /// <returns>The description of the model.</returns>
        protected override string ComputeDescription()
        {
            return this.PrototypeCache.Display != null
                       ? this.PrototypeCache.Display.GetDescription()
                       : base.ComputeDescription();
        }
    }
}