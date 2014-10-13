namespace Waffle
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Waffle.Queuing.Azure;

    /// <summary>
    /// Provides extension methods for Redis message queueing.
    /// </summary>
    public static class AzureMessageQueueExtensions
    {
        /// <summary>
        ///  Enables Azure message queuing.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/>.</param>
        /// <param name="connectionString">The Azure connection string.</param>
        /// <remarks>        
        /// The runner count is <see cref="M:Environment.ProcessorsCount"/>.
        /// </remarks>
        public static void EnableAzureMessageQueuing(this ProcessorConfiguration configuration, string connectionString)
        {
            int runnerCount = Environment.ProcessorCount;
            configuration.EnableAzureMessageQueuing(connectionString, runnerCount);
        }

        /// <summary>
        ///  Enables Azure message queuing.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/>.</param>
        /// <param name="connectionString">The Azure connection string.</param>
        /// <param name="runnerCount">The number of parallel runners.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Dispose is made later by ProcessorConfiguration.RegisterForDispose().")]
        public static void EnableAzureMessageQueuing(this ProcessorConfiguration configuration, string connectionString, int runnerCount)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            AzureCommandQueue azureQueue = null;
            try
            {
                azureQueue = new AzureCommandQueue(connectionString);
                configuration.EnableMessageQueuing(runnerCount, azureQueue, azureQueue);
                azureQueue = null;
            }
            finally
            {
                if (azureQueue != null)
                {
                    azureQueue.Dispose();
                }
            }
        }
    }
}
