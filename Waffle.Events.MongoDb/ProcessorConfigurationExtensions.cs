namespace Waffle
{
    using System;
    using Waffle.Events;
    using Waffle.Events.MongoDb;

    public static class ProcessorConfigurationExtensions
    {
        public static IEventStore EnableMongoEventSourcing(this ProcessorConfiguration configuration, string connectionString, string database)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            MongoEventStore eventStore = new MongoEventStore(connectionString, database);

            configuration.Services.Replace(typeof(IEventStore), eventStore);

            return eventStore;
        }
    }
}
