namespace CommandProcessing.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        public void WhenInstanciatingDbQueryContextThenReturnsObject()
        {
            // Arrange
            var context = CreateDbContext();

            // Act
            DbQueryContext queryContext = new DbQueryContext(context);

            // Assert
            Assert.IsNotNull(context);
        }

        [TestMethod]
        public void WhenFindingItemThenRetunsEntity()
        {
            // Arrange
            var context = CreateDbContext(10);
            DbQueryContext queryContext = new DbQueryContext(context);

            // Act & assert
            var result = queryContext.Find<FakeEntity>("test3");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Property2);
            Assert.AreEqual("test3", result.Property1);
        }

        [TestMethod]
        public void WhenQueryingItemsThenRetunsEntities()
        {
            // Arrange
            var context = CreateDbContext(10);
            DbQueryContext queryContext = new DbQueryContext(context);

            // Act & assert
            var query = queryContext.Query<FakeEntity>();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(context.Entities.Count(), query.Count());
            Assert.IsNotNull(query.FirstOrDefault());
            Assert.AreEqual(context.Entities.FirstOrDefault(), query.FirstOrDefault());
            Assert.AreEqual(context.Entities.Count(item => item.Property2 >= 5), query.Count(item => item.Property2 >= 5));
        }

        private static FakeDbContext CreateDbContext(int count = 0)
        {
            DbConnection connection = Effort.DbConnectionFactory.CreateTransient();

            FakeDbContext context = new FakeDbContext(connection);
            for (int i = 1; i <= count; i++)
            {
                context.Entities.Add(new FakeEntity { Property1 = "test" + i, Property2 = i });
            }

            context.SaveChanges();
            return context;
        }

        private static DbSet<T> CreateSet<T>() where T : class
        {
            return Activator.CreateInstance<DbSet<T>>();
        }
    }
}