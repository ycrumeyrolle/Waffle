namespace CommandProcessing.Metadata
{
    public static class MetadataHelper
    {
        internal static string CreatePropertyModelName(string prefix, string propertyName)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return propertyName ?? string.Empty;
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                return prefix;
            }

            return prefix + "." + propertyName;
        }

        internal static string CreateIndexModelName(string parentName, string index)
        {
            return (parentName.Length == 0) ? "[" + index + "]" : parentName + "[" + index + "]";
        }
    }
}
