namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;

    public interface ICommandSender
    {
        bool IsCompleted { get; }

        Task SendAsync(ICommand command, CancellationToken cancellationToken);

        void Complete();
    }
}
