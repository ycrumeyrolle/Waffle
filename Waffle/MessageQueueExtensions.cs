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
        /// The runner count is <see cref="M:Environment.ProcessorsCount"/>.
        /// </remarks>
        public static void EnableInMemoryMessageQueuing(this ProcessorConfiguration configuration)
        {
            int runnerCount = Environment.ProcessorCount;
            configuration.EnableInMemoryMessageQueuing(runnerCount);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Dispose is made later by ProcessorConfiguration.RegisterForDispose().")]
        public static void EnableInMemoryMessageQueuing(this ProcessorConfiguration configuration, int runnerCount)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            var innerWorker = configuration.Services.GetCommandWorker();
            configuration.Services.Replace(typeof(ICommandWorker), new CommandQueueWorker(innerWorker));

            InMemoryCommandQueue inMemoryQueue = null;
            try
            {
                inMemoryQueue = new InMemoryCommandQueue();
                configuration.Services.Replace(typeof(ICommandSender), inMemoryQueue);
                configuration.Services.Replace(typeof(ICommandReceiver), inMemoryQueue);
                configuration.RegisterForDispose(inMemoryQueue);
                inMemoryQueue = null;
            }
            finally
            {
                if (inMemoryQueue != null)
                {
                    inMemoryQueue.Dispose();
                }
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
                    var receiver = originalConfig.Services.GetCommandReceiver();
                    originalConfig.CommandBroker = new CommandRunner(processor, receiver, runnerCount);
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
