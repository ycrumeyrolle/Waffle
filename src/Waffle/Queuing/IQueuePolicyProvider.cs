namespace Waffle.Queuing
{
    using Waffle.Dependencies;

    /// <summary>
    /// Provides an abstraction for providing a <see cref="QueuePolicy"/>.
    /// A different implementation can be registered via the <see cref="IDependencyResolver"/>. 
    /// </summary>
    public interface IQueuePolicyProvider
    {
        /// <summary>
        /// Gets the <see cref="QueuePolicy"/>.
        /// </summary>
       QueuePolicy QueuePolicy { get; }
    }
}
