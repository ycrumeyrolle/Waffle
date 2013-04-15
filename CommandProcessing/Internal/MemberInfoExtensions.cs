namespace CommandProcessing.Internal
{
    using System.Reflection;

    internal static class MemberInfoExtensions
    {
        public static TAttribute[] GetCustomAttributes<TAttribute>(this MemberInfo member, bool inherit) where TAttribute : class
        {
            if (member == null)
            {
                throw Error.ArgumentNull("member");
            }

            return (TAttribute[])member.GetCustomAttributes(typeof(TAttribute), inherit);
        }
    }
}