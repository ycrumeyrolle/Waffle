namespace Waffle.Tests.Queries
{
    using System;
    using Xunit;
    using Moq;
    using Waffle.Queries;
    using Waffle.Tests.Helpers;
    
    public class DefaultQueryServiceTests
    {
        private readonly Mock<IQueryContext> queryContext = new Mock<IQueryContext>(MockBehavior.Strict);

        [Fact]
        public void WhenRegisteringNullTypeThenThrowsArgumentNullException()
        {
            // Arrange
            var queryService = CreateService();

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => queryService.RegisterContextFactory(null, () => this.queryContext.Object), "contextType");
        }

        [Fact]
        public void WhenRegisteringNullContextFactoryThenThrowsArgumentNullException()
        {
            // Arrange
            var queryService = CreateService();

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => queryService.RegisterContextFactory<IQueryContext>(typeof(object), null), "queryContextFactory");
        }

        [Fact]
        public void WhenRegisteringContextFactoryThenFactoryIsRegistered()
        {
            // Arrange
            var queryService = CreateService();

            // Act & assert
            queryService.RegisterContextFactory(this.queryContext.Object.GetType(), () => this.queryContext.Object);
        }

        [Fact]
        public void WhenCreatingWitrhNullTypeThenThrowArgumentNullException()
        {
            // Arrange
            var queryService = CreateService();

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => queryService.CreateContext(null), "contextType");
        }

        [Fact]
        public void WhenCreatingUnknowContextThenThrowInvalidOperationException()
        {
            // Arrange
            var queryService = CreateService();

            // Act & assert
            Assert.Throws<InvalidOperationException>(() => queryService.CreateContext(this.queryContext.Object.GetType()));
        }

        [Fact]
        public void WhenCreatingContextReturnNullThenThrowInvalidOperationException()
        {
            // Arrange
            var queryService = CreateService();
            queryService.RegisterContextFactory<IQueryContext>(this.queryContext.Object.GetType(), () => null);

            // Act & assert
            Assert.Throws<InvalidOperationException>(() => queryService.CreateContext(this.queryContext.Object.GetType()));
        }

        [Fact]
        public void WhenCreatingContextThenReturnsContext()
        {
            // Arrange
            var queryService = CreateService();
            queryService.RegisterContextFactory(this.queryContext.Object.GetType(), () => this.queryContext.Object);

            // Act 
            IQueryContext result = queryService.CreateContext(this.queryContext.Object.GetType());

            Assert.NotNull(result);
            Assert.IsType(this.queryContext.Object.GetType(), result);
        }

        private static DefaultQueryService CreateService()
        {
            DefaultQueryService queryService = new DefaultQueryService();
            return queryService;
        }
    }
}