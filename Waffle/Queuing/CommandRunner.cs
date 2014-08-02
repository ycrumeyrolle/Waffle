namespace Waffle.Queuing
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Internal;

    /// <summary>
    /// Represents a command runner.
    /// </summary>
    public sealed class CommandRunner : ICommandRunner, IDisposable
    {
        private readonly CommandQueueRunner[] runners;

        private readonly IMessageProcessor processor;

        private readonly ICommandReceiver receiver;

        private bool started;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRunner"/> class. 
        /// </summary>
        /// <param name="processor">The <see cref="IMessageProcessor"/>.</param>
        /// <param name="receiver">The <see cref="ICommandReceiver"/>.</param>
        /// <param name="degreeOfParallelism">The maximum degree of parallelism.</param>
        public CommandRunner(IMessageProcessor processor, ICommandReceiver receiver, int degreeOfParallelism)
        {
            if (processor == null)
            {
                throw new ArgumentNullException("processor");
            }

            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }

            if (degreeOfParallelism <= 0)
            {
                throw new ArgumentOutOfRangeException("degreeOfParallelism");
            }

            this.runners = new CommandQueueRunner[degreeOfParallelism];
            this.processor = processor;
            this.receiver = receiver;
        }

        /// <inheritdocs />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (this.started)
            {
                throw Error.InvalidOperation("The runner is already started.");
            }

            this.started = true;
            var tasks = new Task[this.runners.Length];
            Task task;
            try
            {
                for (int i = 0; i < this.runners.Length; i++)
                {
                    var runner = new CommandQueueRunner(this.processor, this.receiver);
                    this.runners[i] = runner;
                    tasks[i] = runner.RunAsync(cancellationToken);
                }
            }
            finally
            {
                task = Task.WhenAll(tasks.Where(t => t != null));
            }

            await task;
            this.started = false;
        }

        /// <summary>
        /// Stops the runner.
        /// </summary>
        public void Stop()
        {
            this.receiver.Complete();
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
