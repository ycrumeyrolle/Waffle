namespace Waffle
{
    using System.Diagnostics.CodeAnalysis;
    using Waffle.Commands;
    using Waffle.Internal;
    using Waffle.Queuing;

    public static class MessageQueueExtensions
    {
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This has to be made by the caller.")]
        public static CommandBroker EnableMessageQueuing(this ProcessorConfiguration configuration, int runnerCount)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            configuration.Services.Replace(typeof(ICommandWorker), new CommandQueueWorker());

            var processor = configuration.Services.GetProcessor();
            var receiver = configuration.Services.GetCommandReceiver();
            CommandBroker broker = new CommandBroker(processor, receiver, runnerCount);
            configuration.RegisterForDispose(broker);

            return broker;
        }
    }
}
