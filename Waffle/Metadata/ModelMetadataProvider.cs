namespace Waffle.Metadata
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides an abstract class to implement a metadata provider.
    /// </summary>
    public abstract class ModelMetadataProvider
    {
        /// <summary>
        /// Gets a ModelMetadata object for each property of a model.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="containerType">The type of the container.</param>
        /// <returns>A ModelMetadata object for each property of a model.</returns>
        public abstract IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType);

        /// <summary>
        /// A ModelMetadata object for each property of a model.
        /// </summary>
        /// <param name="modelAccessor">The model accessor.</param>
        /// <param name="modelType">The type of the mode.</param>
        /// <returns>The metadata.</returns>
        public abstract ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType);
    }
}
