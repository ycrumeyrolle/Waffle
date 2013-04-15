namespace CommandProcessing.Metadata
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a flattened model.
    /// </summary>
    /// <remarks>
    /// Properties are stored as a key-value dictionary.  
    /// </remarks>
    [Serializable]
    public class ModelDictionary : Dictionary<string, object>
    {
    }
}
