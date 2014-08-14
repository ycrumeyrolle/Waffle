namespace Waffle
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Waffle.Queuing.Redis;

    /// <summary>
    /// Provides extension methods for Redis message queueing.
    /// </summary>
    public static class RedisMessageQueueExtensions
    {
        /// <summary>
        ///  Enables Redis message queuing.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/>.</param>
        /// <param name="redisConfiguration">The Redis configuration string.</param>
        /// <remarks>        
        /// The runner count is <see cref="M:Environment.ProcessorsCount"/>.
        /// </remarks>
        public static void EnableRedisMessageQueuing(this ProcessorConfiguration configuration, string redisConfiguration)
        {
            int runnerCount = Environment.ProcessorCount;
            configuration.EnableRedisMessageQueuing(redisConfiguration, runnerCount);
        }

        /// <summary>
        ///  Enables Redis message queuing.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/>.</param>
        /// <param name="redisConfiguration">The Redis configuration string.</param>
        /// <param name="runnerCount">The number of parallel runners.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Dispose is made later by ProcessorConfiguration.RegisterForDispose().")]
        public static void EnableRedisMessageQueuing(this ProcessorConfiguration configuration, string redisConfiguration, int runnerCount)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            RedisCommandQueue redisQueue = null;
            try
            {
                redisQueue = new RedisCommandQueue(redisConfiguration);
                configuration.EnableMessageQueuing(runnerCount, redisQueue, redisQueue);
                redisQueue = null;
            }
            finally
            {
                if (redisQueue != null)
                {
                    redisQueue.Dispose();
                }
            }
        }
    }
}
