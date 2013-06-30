namespace Waffle.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a flattened model.
    /// </summary>
    /// <remarks>
    /// Properties are stored as a key-value dictionary.  
    /// </remarks>
    [Serializable]
    public class ModelDictionary : Dictionary<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelDictionary"/>.
        /// </summary>
        public ModelDictionary()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelDictionary"/> class with serialized data.
        /// </summary>
        /// <param name="info">
        /// A <see cref="SerializationInfo"/> object containing the information required to serialize the <see cref="ModelDictionary"/>.
        /// </param>
        /// <param name="context">
        /// A <see cref="StreamingContext"/> structure containing the source and destination of the serialized stream associated with the <see cref="ModelDictionary"/>.
        /// </param>
        protected ModelDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
