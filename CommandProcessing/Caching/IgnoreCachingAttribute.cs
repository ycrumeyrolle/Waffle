namespace CommandProcessing.Caching
{
    using System;

    /// <summary>
    /// Represents an attribute to mark a property to be ignored in chacing mecanism.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class IgnoreCachingAttribute : Attribute
    {
    }
}
