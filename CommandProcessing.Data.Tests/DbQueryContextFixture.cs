namespace CommandProcessing.Data.Tests
{
    using System.Data.Common;
    using System.Linq;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DbQueryContextFixture
    {
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
            FakeDbContext context = CreateDbContext();

            // Act
            DbQueryContext queryContext = new DbQueryContext(context);

            // Assert
            Assert.IsNotNull(queryContext);
        }

        [TestMethod]
        public void WhenFindingItemThenRetunsEntity()
        {
            // Arrange
            FakeDbContext context = CreateDbContext(10);
            DbQueryContext queryContext = new DbQueryContext(context);

            // Act
            FakeEntity result = queryContext.Find<FakeEntity>("test3");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Property2);
            Assert.AreEqual("test3", result.Property1);
        }

        [TestMethod]
        public void WhenQueryingItemsThenRetunsEntities()
        {
            // Arrange
            FakeDbContext context = CreateDbContext(10);
            DbQueryContext queryContext = new DbQueryContext(context);

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
            DbQueryContext queryContext = new DbQueryContext(context);

            // Act
            queryContext.Dispose();

            // Assert
            Assert.IsTrue(context.Disposed);
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
    }
}