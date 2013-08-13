namespace Waffle
{
    using System;
    using Waffle.Events;
    using Waffle.Events.MongoDb;

    /// <summary>
    /// This static class contains helper methods related to the registration
    /// of <see cref="MongoEventStore"/> instances.
    /// </summary>
    public static class ProcessorConfigurationExtensions
    {
        /// <summary>
        /// Creates and registers an <see cref="MongoEventStore"/> implementation to use
        /// for this application.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/> for which to register the created trace writer.</param>
        /// <param name="connectionString">Thye connection string.</param>
        /// <param name="databaseName">The database name.</param> 
        /// <returns>The <see cref="MongoEventStore"/> which was created and registered.</returns>
        public static MongoEventStore EnableMongoEventSourcing(this ProcessorConfiguration configuration, string connectionString, string databaseName)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            MongoEventStore eventStore = new MongoEventStore(connectionString, databaseName);

            configuration.Services.Replace(typeof(IEventStore), eventStore);

            return eventStore;
        }
    }
}
