namespace Waffle.Queries.Data.Tests
{
    using System.Data.Common;
    using System.Data.Entity;

    public class FakeDbContext : DbContext
    {
        public FakeDbContext(DbConnection connection)
            : base(connection, true)
        {
        }

        public DbSet<FakeEntity> Entities { get; set; }

        public bool Disposed { get; set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.Disposed = true;
        }
    }
}