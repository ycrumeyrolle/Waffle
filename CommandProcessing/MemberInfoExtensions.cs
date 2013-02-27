namespace CommandProcessing
{
    using System;
    using System.Reflection;

    internal static class MemberInfoExtensions
    {
        public static TAttribute[] GetCustomAttributes<TAttribute>(this MemberInfo member, bool inherit) where TAttribute : class
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }

            return (TAttribute[])member.GetCustomAttributes(typeof(TAttribute), inherit);
        }
    }
}