namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICommandQueueRunner
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
