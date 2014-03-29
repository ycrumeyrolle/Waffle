using Effort;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Waffle.Queries;
using Waffle.Sample.Controllers;

namespace Waffle.Sample
{
    public static class WaffleConfig
    {
        public static void Register(ProcessorConfiguration config)
        {
            config.RegisterContextFactory(() => new ConferenceDbContext(DbConnectionFactory.CreatePersistent("1")));
        }
    }
}