namespace Waffle.Tests.Queries
{
    using System;
    using Xunit;
    using Moq;
    using Waffle.Queries;
    using Waffle.Tests.Helpers;
    
    public class QueryServiceExtensionsTests
    {
        private readonly Mock<IQueryService> queryService = new Mock<IQueryService>(MockBehavior.Strict);

        private readonly Mock<IQueryContext> queryContext = new Mock<IQueryContext>(MockBehavior.Strict);

        [Fact]
        public void WhenRegisteringWithNullServiceThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => QueryServiceExtensions.RegisterContextFactory(null, () => this.queryContext.Object), "queryService");
        }

        [Fact]
        public void WhenRegisteringContextFactoryThenRegistrationIsDelegated()
        {
            // Arrange
            this.queryService.Setup(s => s.RegisterContextFactory(It.IsAny<Type>(), It.IsAny<Func<IQueryContext>>()));

            // Act 
            QueryServiceExtensions.RegisterContextFactory<IQueryContext>(this.queryService.Object, () => this.queryContext.Object);

            // Assert
            this.queryService.Verify(s => s.RegisterContextFactory(It.IsAny<Type>(), It.IsAny<Func<IQueryContext>>()), Times.Once());
        }

        [Fact]
        public void WhenCreatingContextWithNullServiceThenThrowInvalidOperationException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => QueryServiceExtensions.CreateContext<IQueryContext>(null), "queryService");
        }

        [Fact]
        public void WhenCreatingContextThenCreationIsDelegated()
        {
            // Arrange
            this.queryService.Setup(s => s.CreateContext(It.IsAny<Type>())).Returns(this.queryContext.Object);

            // Act 
            IQueryContext result = QueryServiceExtensions.CreateContext<IQueryContext>(this.queryService.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType(this.queryContext.Object.GetType(), result);
            this.queryService.Verify(s => s.CreateContext(It.IsAny<Type>()), Times.Once());
        }
    }
}