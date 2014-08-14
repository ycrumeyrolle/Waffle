namespace Waffle
{
    using System;

    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    internal sealed class HandlerAttribute : Attribute
    {
    }
}
