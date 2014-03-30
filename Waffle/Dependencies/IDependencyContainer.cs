namespace Waffle.Dependencies
{
    using System;
    using System.Collections.Generic;

    public interface IDependencyContainer : IDisposable
    {
        object Resolve(Type type);

        IEnumerable<object> ResolveAll(Type type);

        void RegisterInstance(Type type, object instance);

        void RegisteType(Type fromType, Type toType);
    }
}
