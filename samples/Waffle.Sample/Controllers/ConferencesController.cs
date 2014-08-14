namespace Waffle.Sample.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Http;
    using Waffle.Queries;
    using Waffle.Queries.Data;

    public class ConferencesController : ApiController
    {
        private readonly IQueryService queryService;
        
        public ConferencesController(IQueryService queryService)
        {
            this.queryService = queryService;
        }

        public IEnumerable<ConferenceInfo> Get()
        {
            using (var context = this.queryService.CreateContext<DbQueryContext<ConferenceDbContext>>())
            {
                return context.Query<ConferenceEntity>()
                    .Select(c => new ConferenceInfo { Name = c.Name, Description = c.Description });
            }
        }
    }
}