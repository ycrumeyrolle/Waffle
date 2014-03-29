namespace Waffle.Queries.Data.Tests
{
    using System.Data.Common;
    using System.Linq;
    using Effort;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Waffle.Tests.Helpers;

    [TestClass]
    public class DbQueryContextFixture
    {
        [TestMethod]
        public void WhenInstanciatingDbQueryContextWithoutDbContextThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => new DbQueryContext<FakeDbContext>(null), "innerContext");
        }

        [TestMethod]
        public void WhenInstanciatingDbQueryContextThenReturnsObject()
        {
            // Arrange
            FakeDbContext context = CreateDbContext();

            // Act
            DbQueryContext<FakeDbContext> queryContext = new DbQueryContext<FakeDbContext>(context);

            // Assert
            Assert.IsNotNull(queryContext);
        }

        [TestMethod]
        public void WhenFindingItemThenRetunsEntity()
        {
            // Arrange
            FakeDbContext context = CreateDbContext(10);
            DbQueryContext<FakeDbContext> queryContext = new DbQueryContext<FakeDbContext>(context);

            // Act
            FakeEntity result = queryContext.Find<FakeEntity>("test3");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Property2);
            Assert.AreEqual("test3", result.Property1);
        }

        [TestMethod]
        public void WhenQueryingItemsThenReturnsEntities()
        {
            // Arrange
            FakeDbContext context = CreateDbContext(10);
            DbQueryContext<FakeDbContext> queryContext = new DbQueryContext<FakeDbContext>(context);

            // Act
            var query = queryContext.Query<FakeEntity>();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(context.Entities.Count(), query.Count());
            Assert.IsNotNull(query.FirstOrDefault());
            Assert.AreEqual(context.Entities.FirstOrDefault(), query.FirstOrDefault());
            Assert.AreEqual(context.Entities.Count(item => item.Property2 >= 5), query.Count(item => item.Property2 >= 5));
        }

        [TestMethod]
        public void WhenDisposingQueryContexthenDisposeInnerDbContext()
        {
            // Arrange
            FakeDbContext context = CreateDbContext(10);
            DbQueryContext<FakeDbContext> queryContext = new DbQueryContext<FakeDbContext>(context);

            // Act
            queryContext.Dispose();

            // Assert
            Assert.IsTrue(context.Disposed);
        }

        private static FakeDbContext CreateDbContext(int count = 0)
        {
            DbConnection connection = DbConnectionFactory.CreateTransient();

            FakeDbContext context = new FakeDbContext(connection);
            for (int i = 1; i <= count; i++)
            {
                context.Entities.Add(new FakeEntity { Property1 = "test" + i, Property2 = i });
            }

            context.SaveChanges();
            return context;
        }
    }
}