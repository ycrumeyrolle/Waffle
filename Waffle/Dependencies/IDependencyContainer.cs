using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waffle.Dependencies
{
    public interface IDependencyContainer : IDisposable
    {
        object Resolve(Type type);

        IEnumerable<object> ResolveAll(Type type);

        void RegisterInstance(Type type, object instance);

        void RegisteType(Type fromType, Type toType);
    }
}
