namespace Waffle.Queries.Data.Tests
{
    using System.Data.Common;
    using System.Linq;
    using System.Threading.Tasks;
    using Effort;
    using Waffle.Tests.Helpers;
    using Xunit;
    
    public class DbQueryContextTests
    {
        [Fact]
        public void WhenInstanciatingDbQueryContextWithoutDbContextThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => new DbQueryContext<FakeDbContext>(null), "innerContext");
        }

        [Fact]
        public void WhenInstanciatingDbQueryContextThenReturnsObject()
        {
            // Arrange
            FakeDbContext context = CreateDbContext();

            // Act
            DbQueryContext<FakeDbContext> queryContext = new DbQueryContext<FakeDbContext>(context);

            // Assert
            Assert.NotNull(queryContext);
        }

        [Fact]
        public async Task WhenFindingItemThenRetunsEntity()
        {
            // Arrange
            FakeDbContext context = CreateDbContext(10);
            DbQueryContext<FakeDbContext> queryContext = new DbQueryContext<FakeDbContext>(context);

            // Act
            FakeEntity result = await queryContext.FindAsync<FakeEntity>("test3");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Property2);
            Assert.Equal("test3", result.Property1);
        }

        [Fact]
        public void WhenQueryingItemsThenReturnsEntities()
        {
            // Arrange
            FakeDbContext context = CreateDbContext(10);
            DbQueryContext<FakeDbContext> queryContext = new DbQueryContext<FakeDbContext>(context);

            // Act
            var query = queryContext.Query<FakeEntity>();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(context.Entities.Count(), query.Count());
            Assert.NotNull(query.FirstOrDefault());
            Assert.Equal(context.Entities.FirstOrDefault(), query.FirstOrDefault());
            Assert.Equal(context.Entities.Count(item => item.Property2 >= 5), query.Count(item => item.Property2 >= 5));
        }

        [Fact]
        public void WhenDisposingQueryContexthenDisposeInnerDbContext()
        {
            // Arrange
            FakeDbContext context = CreateDbContext(10);
            DbQueryContext<FakeDbContext> queryContext = new DbQueryContext<FakeDbContext>(context);

            // Act
            queryContext.Dispose();

            // Assert
            Assert.True(context.Disposed);
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