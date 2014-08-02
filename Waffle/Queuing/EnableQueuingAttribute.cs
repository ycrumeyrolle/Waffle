namespace Waffle.Queuing
{
    using System;

    /// <summary>
    /// Specifies the activation of the queueing behaviour for a specific command handler.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class EnableQueuingAttribute : Attribute, IQueuePolicyProvider
    {
      
        /// <inheritdocs />
        public QueuePolicy QueuePolicy
        {
            get { return QueuePolicy.Queue; }
        }
    }
}
