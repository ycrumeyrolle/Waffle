namespace CommandProcessing.Internal
{
    using System;
    using System.Linq.Expressions;

    internal static class TypeActivator
    {
        public static Func<TBase> Create<TBase>(Type instanceType) where TBase : class
        {
            NewExpression body = Expression.New(instanceType);
            return Expression.Lambda<Func<TBase>>(body, new ParameterExpression[0]).Compile();
        }
    }
}