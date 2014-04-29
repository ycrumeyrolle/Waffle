namespace Waffle
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    
    public interface ICommandReceiver
    {
        bool IsCompleted { get; }

        Task<ICommand> ReceiveAsync(CancellationToken cancellationToken);

        void Complete();
    }
}
