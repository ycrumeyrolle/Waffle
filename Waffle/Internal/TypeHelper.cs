namespace Waffle.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Events;

    internal static class TypeHelper
    {
        private static readonly Type TaskGenericType = typeof(Task<>);

        internal static readonly Type CommandHandlerType = typeof(ICommandHandler);

        internal static readonly Type EventHandlerType = typeof(IEventHandler);

        internal static readonly Type ExceptionType = typeof(Exception);
        
        internal static Type GetTaskInnerTypeOrNull(Type type)
        {
            Contract.Requires(type != null);
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();

                if (TaskGenericType == genericTypeDefinition)
                {
                    return type.GetGenericArguments()[0];
                }
            }

            return null;
        }

        internal static ReadOnlyCollection<T> OfType<T>(object[] objects) where T : class
        {
            Contract.Requires(objects != null); 
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

        internal static bool HasStringConverter(Type type)
        {
            Contract.Requires(type != null);
            return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
        }

        internal static bool IsNullableValueType(Type type)
        {
            Contract.Requires(type != null); 
            return Nullable.GetUnderlyingType(type) != null;
        }

        internal static bool IsSimpleType(Type type)
        {
            Contract.Requires(type != null); 
            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(DateTime) ||
                   type == typeof(decimal) ||
                   type == typeof(Guid) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(TimeSpan);
        }
    }
}