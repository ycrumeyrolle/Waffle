﻿namespace Waffle
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Waffle.Internal;

    /// <summary>
    /// Helper extension methods for fast use of collections.
    /// </summary>
    internal static class CollectionExtensions
    {      
        /// <summary>
        /// Return the enumerable as an typed Array, copying if required. Optimized for common case where it is an Array. 
        /// Avoid mutating the return value.
        /// </summary>
        public static T[] AsArray<T>(this IEnumerable<object> values)
        {
            Contract.Requires(values != null);

            object[] array = values as object[];

            if (array != null)
            {
                return Array.ConvertAll(array, item => (T)item);
            }

            return values.Cast<T>().ToArray();
        }

        /// <summary>
        /// Return the enumerable as an Array, copying if required. Optimized for common case where it is an Array. 
        /// Avoid mutating the return value.
        /// </summary>
        public static T[] AsArray<T>(this IEnumerable<T> values)
        {
            Contract.Requires(values != null);

            T[] array = values as T[] ?? values.ToArray();

            return array;
        }

        /// <summary>
        /// Return the enumerable as a Collection of T, copying if required. Optimized for the common case where it is 
        /// a Collection of T and avoiding a copy if it implements IList of T. Avoid mutating the return value.
        /// </summary>
        public static Collection<T> AsCollection<T>(this IEnumerable<T> enumerable)
        {
            Contract.Requires(enumerable != null);

            Collection<T> collection = enumerable as Collection<T>;
            if (collection != null)
            {
                return collection;
            }

            // Check for IList so that collection can wrap it instead of copying
            IList<T> list = enumerable as IList<T> ?? new List<T>(enumerable);

            return new Collection<T>(list);
        }

        /// <summary>
        /// Return the enumerable as a IList of T, copying if required. Avoid mutating the return value.
        /// </summary>
        public static IList<T> AsIList<T>(this IEnumerable<T> enumerable)
        {
            Contract.Requires(enumerable != null);

            IList<T> list = enumerable as IList<T>;
            if (list != null)
            {
                return list;
            }

            return new List<T>(enumerable);
        }
        
        /// <summary>
        /// Return the enumerable as a List of T, copying if required. Optimized for common case where it is an List of T 
        /// or a ListWrapperCollection of T. Avoid mutating the return value.
        /// </summary>
        public static List<T> AsList<T>(this IEnumerable<T> enumerable)
        {
            Contract.Requires(enumerable != null);

            List<T> list = enumerable as List<T>;
            if (list != null)
            {
                return list;
            }

            ListWrapperCollection<T> listWrapper = enumerable as ListWrapperCollection<T>;
            if (listWrapper != null)
            {
                return listWrapper.ItemsList;
            }

            return new List<T>(enumerable);
        }

        /// <summary>
        /// Return the only value from list, the type's default value if empty, or call the errorAction for 2 or more.
        /// </summary>
        public static T SingleDefaultOrError<T, TArg1>(this IList<T> list, Action<TArg1> errorAction, TArg1 errorArg1)
        {
            Contract.Requires(list != null);
            Contract.Requires(errorAction != null);

            switch (list.Count)
            {
                case 0:
                    return default(T);

                case 1:
                    T value = list[0];
                    return value;

                default:
                    errorAction(errorArg1);
                    return default(T);
            }
        }

        /// <summary>
        /// Returns a single value in list matching type TMatch if there is only one, null if there are none of type TMatch or calls the
        /// errorAction with errorArg1 if there is more than one.
        /// </summary>
        public static TMatch SingleOfTypeDefaultOrError<TInput, TMatch, TArg1>(this IList<TInput> list, Action<TArg1> errorAction, TArg1 errorArg1) where TMatch : class
        {
            Contract.Requires(list != null);
            Contract.Requires(errorAction != null);

            TMatch result = null;
            for (int i = 0; i < list.Count; i++)
            {
                TMatch typedValue = list[i] as TMatch;
                if (typedValue != null)
                {
                    if (result == null)
                    {
                        result = typedValue;
                    }
                    else
                    {
                        errorAction(errorArg1);
                        return null;
                    }
                }
            }

            return result;
        }
       
        /// <summary>
        /// Returns a single value in list matching type TMatch if there is only one, null if there are none of type TMatch or calls the
        /// errorAction with errorArg1 if there is more than one.
        /// </summary>
        public static TMatch FirstOrDefaultOfType<TInput, TMatch>(this IList<TInput> list, Type matchType) where TMatch : class
        {
            if (list == null)
            {
                throw Error.ArgumentNull("list");
            }

            TMatch result = null;
            for (int i = 0; i < list.Count; i++)
            {
                TMatch typedValue = list[i] as TMatch;
                if (typedValue != null && typedValue.GetType() == matchType)
                {
                    if (result == null)
                    {
                        result = typedValue;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return result;
        }
    }
}
