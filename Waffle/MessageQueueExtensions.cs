namespace Waffle
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Waffle.Commands;
    using Waffle.Internal;
    using Waffle.Queuing;

    /// <summary>
    /// Provides extension methods for message queueing.
    /// </summary>
    public static class MessageQueueExtensions
    {
        /// <summary>
        ///  Enables in-memory message queuing.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/>.</param>
        /// <remarks>        
        /// The maximum degree of parallelism is <see cref="M:Environment.ProcessorsCount"/>.
        /// </remarks>
        public static void EnableInMemoryMessageQueuing(this ProcessorConfiguration configuration)
        {
            int runnerCount = Environment.ProcessorCount;
            configuration.EnableInMemoryMessageQueuing(runnerCount);
        }

        /// <summary>
        ///  Enables in-memory message queuing.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/>.</param>
        /// <param name="degreeOfParallelism">The maximum degree of parallelism.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Dispose is made later by ProcessorConfiguration.RegisterForDispose().")]
        public static void EnableInMemoryMessageQueuing(this ProcessorConfiguration configuration, int degreeOfParallelism)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            InMemoryCommandQueue inMemoryQueue = null;
            try
            {
                inMemoryQueue = new InMemoryCommandQueue();
                configuration.EnableMessageQueuing(degreeOfParallelism, inMemoryQueue, inMemoryQueue);
                inMemoryQueue = null;
            }
            finally
            {
                if (inMemoryQueue != null)
                {
                    inMemoryQueue.Dispose();
                }
            }
        }

        /// <summary>
        ///  Enables message queuing.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/>.</param>
        /// <param name="degreeOfParallelism">The maximum degree of parallelism.</param>
        /// <param name="sender">The <see cref="ICommandSender"/> used to send commands.</param>
        /// <param name="receiver">The <see cref="ICommandReceiver"/> used to receive commands.</param>
        public static void EnableMessageQueuing(this ProcessorConfiguration configuration, int degreeOfParallelism, ICommandSender sender, ICommandReceiver receiver)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            var innerWorker = configuration.Services.GetCommandWorker();
            configuration.Services.Replace(typeof(ICommandWorker), new CommandQueueWorker(innerWorker));

            configuration.Services.Replace(typeof(ICommandSender), sender);
            configuration.Services.Replace(typeof(ICommandReceiver), receiver);
            configuration.RegisterForDispose(sender as IDisposable);
            if (!object.ReferenceEquals(sender, receiver))
            {
                configuration.RegisterForDispose(receiver as IDisposable);
            }

            Action<ProcessorConfiguration> defaultInitializer = configuration.Initializer;

            configuration.Initializer = originalConfig =>
            {
                MessageProcessor processor = null;
                try
                {
                    CommandHandlerSettings settings = new CommandHandlerSettings(originalConfig);
                    settings.Services.Replace(typeof(ICommandWorker), innerWorker);
                    var config = ProcessorConfiguration.ApplyHandlerSettings(settings, originalConfig);
                    config.Initializer = defaultInitializer;
                    processor = new MessageProcessor(config);
                    var commandReceiver = originalConfig.Services.GetCommandReceiver();
                    originalConfig.CommandBroker = new CommandRunner(processor, commandReceiver, degreeOfParallelism);
                    originalConfig.RegisterForDispose(processor);
                    processor = null;
                }
                finally
                {
                    if (processor != null)
                    {
                        processor.Dispose();
                    }
                }

                defaultInitializer(originalConfig);
            };
        }
    }
}
