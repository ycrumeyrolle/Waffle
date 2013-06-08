namespace CommandProcessing.Data.Tests
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Common;
    using System.Data.Entity;
    using CommandProcessing.Data.Tests.TestHelpers;
    using CommandProcessing.Queries;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DbQueryContextFixture
    {
        // private readonly Mock<IQueryContext> queryContext = new Mock<IQueryContext>(MockBehavior.Strict);

        [TestMethod]
        public void WhenIntanciatingDbQueryContextWithoutDbContextThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => new DbQueryContext(null), "innerContext");
        }

        [TestMethod]
        public void WhenInstanciatingDbQueryContextThenRetunsObject()
        {
            // Arrange
            var dbContext = CreateDbContext();

            // Act
            DbQueryContext context = new DbQueryContext(dbContext);

            // Assert
            Assert.IsNotNull(context);
        }

        [TestMethod]
        public void WhenInstanciatingDbQueryContextThenRetunsObjectx()
        {
            // Arrange
            var dbContext = CreateDbContext();
            DbQueryContext context = new DbQueryContext(dbContext);

            // Act & assert
            //// var result = context.Find<FakeEntity>("test");

            // Assert
            //// Assert.IsNotNull(context);
            Assert.Inconclusive("Unable to test DbContext for now without database. More work required...");
        }

        private static DbContext CreateDbContext()
        {
            FakeConnectionFactory.RegisterFactory<FakeDbContext>();

            var connection = new FakeDbConnection("fake");
            FakeDbContext context = new FakeDbContext(connection, true);
            Database.SetInitializer(new DoNotCreateDatabaseInitializer<FakeDbContext>());


            //var context = new Mock<DbContext>(MockBehavior.Strict);
            //context.Setup(c => c.Set(typeof(FakeEntity))).Returns(CreateSet<FakeEntity>());
            return context;
        }

        private static DbSet<T> CreateSet<T>() where T : class
        {
            return Activator.CreateInstance<DbSet<T>>();
        }
    }

    public class FakeDbContext : DbContext
    {
        public FakeDbContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public DbSet<FakeEntity> Entities { get; set; }
    }

    public class FakeEntity
    {
        [Key]
        public string Property1 { get; set; }

        public int Property2 { get; set; }
    }
}