namespace Waffle.Queuing
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class CommandBroker : ICommandQueueRunner, IDisposable
    {
        private readonly CommandQueueRunner[] runners;

        private readonly IMessageProcessor processor;

        private readonly ICommandReceiver receiver;

        public CommandBroker(IMessageProcessor processor, ICommandReceiver receiver, int runnerCount)
        {
            if (processor == null)
            {
                throw new ArgumentNullException("processor");
            }

            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }

            if (runnerCount <= 0)
            {
                throw new ArgumentOutOfRangeException("runnerCount");
            }

            this.runners = new CommandQueueRunner[runnerCount];
            this.processor = processor;
            this.receiver = receiver;
        }

        public Task RunAsync(CancellationToken cancellationToken)
        {
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

            return task;
        }

        public void Complete()
        {
            this.receiver.Complete();
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
                this.Complete();                
            }
        }
    }
}
