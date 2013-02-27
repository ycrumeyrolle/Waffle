namespace CommandProcessing
{
    using System.Collections.Generic;
    using System.Reflection;

    public interface IAssembliesResolver
    {
        ICollection<Assembly> GetAssemblies();
    }
}