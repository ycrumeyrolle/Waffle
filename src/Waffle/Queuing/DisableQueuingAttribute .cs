namespace Waffle.Queuing
{
    using System;

    /// <summary>
    /// Specifies the de-activation of the queueing behaviour for a specific command handler.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class DisableQueuingAttribute : Attribute, IQueuePolicyProvider
    {
        /// <inheritdocs />
        public QueuePolicy QueuePolicy
        {
            get { return QueuePolicy.NoQueue; }
        }
    }
}
