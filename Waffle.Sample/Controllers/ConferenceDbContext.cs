using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Waffle.Queries.Data;

namespace Waffle.Sample.Controllers
{
    public class ConferenceDbContext : DbContext
    {
        public ConferenceDbContext()
            : base("sample")
        {
        }

        public ConferenceDbContext(DbConnection connection)
            : base(connection, true)
        {
        }
        
        public IDbSet<ConferenceEntity> Conferences { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var conferences = modelBuilder.Entity<ConferenceEntity>();
                conferences.ToTable("Conferences");
                conferences.HasKey(c => c.Id);

            conferences.Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            conferences.Property(c => c.Name).HasMaxLength(100);
            conferences.Property(c => c.Description).HasMaxLength(1000);

            base.OnModelCreating(modelBuilder);
        }
    }
}
