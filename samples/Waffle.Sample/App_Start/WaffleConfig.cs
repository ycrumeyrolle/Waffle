namespace Waffle.Sample
{
    using Effort;
    using Waffle.Sample.Controllers;

    public static class WaffleConfig
    {
        public static void Register(ProcessorConfiguration config)
        {
            config.RegisterContextFactory(() => new ConferenceDbContext(DbConnectionFactory.CreatePersistent("1")));
        }
    }
}