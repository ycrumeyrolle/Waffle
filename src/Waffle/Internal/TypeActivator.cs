namespace Waffle.Internal
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;

    internal static class TypeActivator
    {
        public static Func<TBase> Create<TBase>(Type instanceType) where TBase : class
        {
            Contract.Requires(instanceType != null);
            NewExpression body = Expression.New(instanceType);
            return Expression.Lambda<Func<TBase>>(body, new ParameterExpression[0]).Compile();
        }
    }
}