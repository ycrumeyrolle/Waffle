namespace CommandProcessing.Caching
{
    using System;

    /// <summary>
    /// Represents a filter to bypass cache command result.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NoCacheAttribute : Attribute
    {
    }
}
