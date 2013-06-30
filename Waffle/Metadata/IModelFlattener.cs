namespace Waffle.Metadata
{
    using System;

    /// <summary>
    /// Defines the methods that are required to flatten a <see cref="ICommand"/>.
    /// </summary>
    public interface IModelFlattener
    {
        /// <summary>
        /// Flatten the <paramref name="model"/>.
        /// </summary>
        /// <param name="model">The model to be flattened.</param>
        /// <param name="type">The <see cref="Type"/> to use for flattening.</param>
        /// <param name="metadataProvider">The <see cref="ModelMetadataProvider"/> used to provide the model metadata.</param>
        /// <param name="keyPrefix">The <see cref="string"/> to append to the key for any validation errors.</param>
        /// <returns>The <see cref="ModelDictionary"/>.</returns>
        ModelDictionary Flatten(object model, Type type, ModelMetadataProvider metadataProvider, string keyPrefix);
    }
}