namespace Waffle.Queuing
{
    public interface IQueuePolicyProvider
    {
       QueuePolicy QueuePolicy { get; }
    }
}
