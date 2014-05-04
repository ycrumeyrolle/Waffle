namespace Waffle.Queuing
{
    using System;

    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class DisableQueuingAttribute : Attribute, IQueuePolicyProvider
    {
        public QueuePolicy QueuePolicy
        {
            get { return QueuePolicy.NoQueue; }
        }
    }
}
