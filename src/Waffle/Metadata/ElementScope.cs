namespace Waffle.Metadata
{
    using System.Globalization;

    internal class ElementScope : IKeyBuilder
    {
        public int Index { get; set; }

        public string AppendTo(string prefix)
        {
            string index = this.Index.ToString(CultureInfo.InvariantCulture);
            return (prefix.Length == 0) ? "[" + index + "]" : prefix + "[" + index + "]";
        }
    }
}