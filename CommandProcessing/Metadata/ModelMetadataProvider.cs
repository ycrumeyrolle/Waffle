namespace CommandProcessing.Metadata
{
    using System;
    using System.Collections.Generic;

    public abstract class ModelMetadataProvider
    {
        public abstract IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType);

        public abstract ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType);
    }
}
