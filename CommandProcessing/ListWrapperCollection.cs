namespace CommandProcessing
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// A class that inherits from Collection of T but also exposes its underlying data as List of T for performance.
    /// </summary>
    internal sealed class ListWrapperCollection<T> : Collection<T>
    {
        private readonly List<T> items;

        internal ListWrapperCollection()
            : this(new List<T>())
        {
        }

        internal ListWrapperCollection(List<T> list)
            : base(list)
        {
            this.items = list;
        }

        internal List<T> ItemsList
        {
            get { return this.items; }
        }
    }
}
