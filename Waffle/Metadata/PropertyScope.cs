namespace Waffle.Metadata
{
    internal class PropertyScope : IKeyBuilder
    {
        public string PropertyName { get; set; }

        public string AppendTo(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return this.PropertyName ?? string.Empty;
            }

            if (string.IsNullOrEmpty(this.PropertyName))
            {
                return prefix;
            }

            return prefix + "." + this.PropertyName;
        }
    }
}