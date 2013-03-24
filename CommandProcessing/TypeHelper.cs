namespace CommandProcessing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal static class TypeHelper
    {
        internal static readonly Type HandlerType = typeof(Handler);

        internal static ReadOnlyCollection<T> OfType<T>(object[] objects) where T : class
        {
            int num = objects.Length;
            List<T> list = new List<T>(num);
            int count = 0;
            for (int i = 0; i < num; i++)
            {
                T t = objects[i] as T;
                if (t != null)
                {
                    list.Add(t);
                    count++;
                }
            }

            list.Capacity = count;
            return new ReadOnlyCollection<T>(list);
        }
    }
}