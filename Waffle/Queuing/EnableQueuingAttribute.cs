namespace Waffle.Queuing
{
    using System;

    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class EnableQueuingAttribute : Attribute, IQueuePolicyProvider
    {
        public QueuePolicy QueuePolicy
        {
            get { return QueuePolicy.Queue; }
        }
    }
}
