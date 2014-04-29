namespace Waffle.Queuing
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;

    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This class act as a Queue.")]
    public class InMemoryCommandQueue : ICommandSender, ICommandReceiver, IDisposable
    {
        private readonly BlockingCollection<ICommand> queue;

        public InMemoryCommandQueue(IProducerConsumerCollection<ICommand> collection)
        {
            this.queue = new BlockingCollection<ICommand>(collection);
        }

        public InMemoryCommandQueue()
            : this(new ConcurrentQueue<ICommand>())
        {
        }
        
        public bool IsCompleted
        {
            get { return this.queue.IsCompleted; }
        }

        public Task SendAsync(ICommand command, CancellationToken cancellationToken)
        {
            this.queue.Add(command, cancellationToken);
            return Task.FromResult(0);
        }

        public Task<ICommand> ReceiveAsync(CancellationToken cancellationToken)
        {
            ICommand command;
            if (this.queue.TryTake(out command, -1, cancellationToken))
            {
                return Task.FromResult(command);
            }

            return Task.FromResult<ICommand>(null);
        }

        public void Complete()
        {
            this.queue.CompleteAdding();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.queue.Dispose();
            }
        }
    }
}
