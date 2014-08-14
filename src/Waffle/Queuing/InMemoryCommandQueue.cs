namespace Waffle.Queuing
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Tasks;

    /// <summary>
    /// Represents an in-memory message queue.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This class act as a Queue.")]
    public class InMemoryCommandQueue : ICommandSender, ICommandReceiver, IDisposable
    {
        private readonly BlockingCollection<ICommand> queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryCommandQueue"/> class.
        /// </summary> 
        /// <param name="collection">The collection to use as the underlying data store.</param>
        public InMemoryCommandQueue(IProducerConsumerCollection<ICommand> collection)
        {
            this.queue = new BlockingCollection<ICommand>(collection);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryCommandQueue"/> class.
        /// </summary> 
        public InMemoryCommandQueue()
            : this(new ConcurrentQueue<ICommand>())
        {
        }

        /// <summary>
        /// Gets whether this <see cref="InMemoryCommandQueue"/> has been marked as complete for adding and is empty. 
        /// </summary>
        public bool IsCompleted
        {
            get { return this.queue.IsCompleted; }
        }

        /// <inheritsdoc />
        public Task SendAsync(ICommand command, CancellationToken cancellationToken)
        {
            this.queue.Add(command, cancellationToken);
            return Task.FromResult(0);
        }

        /// <inheritsdoc />
        public Task<ICommand> ReceiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                ICommand command = this.queue.Take(cancellationToken);
                return Task.FromResult(command);
            }
            catch (OperationCanceledException)
            {
                return Task.FromResult<ICommand>(null);
            }
        }

        /// <summary>
        /// Marks the <see cref="InMemoryCommandQueue"/> instance as not accepting any more additions.
        /// </summary>
        public void Complete()
        {
            this.queue.CompleteAdding();
        }

        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.queue.Dispose();
            }
        }
    }
}
