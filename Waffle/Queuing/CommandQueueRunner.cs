namespace Waffle.Queuing
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Internal;

    public class CommandQueueRunner
    {
        private readonly ICommandReceiver receiver;

        private readonly IMessageProcessor processor;

        public CommandQueueRunner(IMessageProcessor processor, ICommandReceiver receiver)
        {
            if (processor == null)
            {
                throw new ArgumentNullException("processor");
            }

            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }

            this.receiver = receiver;
            this.processor = processor;
        }

        /// <summary>
        /// Runs the <see cref="CommandQueueRunner"/>.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to stop the runner.</param>
        /// <returns>A <see cref="Task"/> of the running <see cref="CommandQueueRunner"/>.</returns>
        public Task RunAsync(CancellationToken cancellationToken)
        {
            Task task = Task.Factory.StartNew(() => this.RunCoreAsync(cancellationToken)).Unwrap();
            return task;
        }

        private async Task RunCoreAsync(CancellationToken cancellationToken)
        {
            while (!this.receiver.IsCompleted && !cancellationToken.IsCancellationRequested)
            {
                await this.ProcessCommandAsync(cancellationToken).ConfigureAwait(true);
            }
        }

        private async Task<HandlerResponse> ProcessCommandAsync(CancellationToken cancellationToken)
        {
            ICommand command = await this.receiver.ReceiveAsync(cancellationToken);

            if (command == null)
            {
                return HandlerResponse.Empty;
            }

            var response = await this.processor.ProcessAsync(command, cancellationToken);

            return response;
        }

        public static CommandQueueRunner Create(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            var processor = configuration.Services.GetServiceOrThrow<IMessageProcessor>();
            var receiver = configuration.Services.GetServiceOrThrow<ICommandReceiver>();

            return new CommandQueueRunner(processor, receiver);
        }
    }
}
