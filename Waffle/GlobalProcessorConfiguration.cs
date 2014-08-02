namespace Waffle
{
    using System;

    /// <summary> 
    /// Provides a global <see cref="T:ProcessorConfiguration" />.
    /// </summary>
    public static class GlobalProcessorConfiguration
    {
        private static Lazy<ProcessorConfiguration> configuration = GlobalProcessorConfiguration.CreateConfiguration();

        private static Lazy<MessageProcessor> defaultProcessor = GlobalProcessorConfiguration.CreateDefaultProcessor();

        /// <summary>
        /// Gets the default <see cref="ProcessorConfiguration"/>.
        /// </summary>
        public static ProcessorConfiguration Configuration
        {
            get
            {
                return GlobalProcessorConfiguration.configuration.Value;
            }
        }

        /// <summary> 
        /// Gets the global <see cref="T:MessageProcessor" />. 
        /// </summary>
        public static MessageProcessor DefaultProcessor
        {
            get
            {
                return GlobalProcessorConfiguration.defaultProcessor.Value;
            }
        }

        /// <summary>
        /// Configures the <see cref="GlobalProcessorConfiguration"/> before its initilization.
        /// </summary>
        /// <param name="configurationCallback"></param>
        public static void Configure(Action<ProcessorConfiguration> configurationCallback)
        {
            if (configurationCallback == null)
            {
                throw new ArgumentNullException("configurationCallback");
            }

            configurationCallback(GlobalProcessorConfiguration.Configuration);
            GlobalProcessorConfiguration.Configuration.EnsureInitialized();
        }

        internal static void Reset()
        {
            GlobalProcessorConfiguration.configuration = GlobalProcessorConfiguration.CreateConfiguration();
            GlobalProcessorConfiguration.defaultProcessor = GlobalProcessorConfiguration.CreateDefaultProcessor();
        }

        private static Lazy<ProcessorConfiguration> CreateConfiguration()
        {
            return new Lazy<ProcessorConfiguration>(delegate
            {
                ProcessorConfiguration config = new ProcessorConfiguration();
                return config;
            });
        }

        private static Lazy<MessageProcessor> CreateDefaultProcessor()
        {
            return new Lazy<MessageProcessor>(() => new MessageProcessor(GlobalProcessorConfiguration.configuration.Value));
        }
    }
}
