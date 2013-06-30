namespace Waffle.Tests.Queries
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Queries;
    using Waffle.Tests.Helpers;

    [TestClass]
    public class QueryServiceExtensionsFixture
    {
        private readonly Mock<IQueryService> queryService = new Mock<IQueryService>(MockBehavior.Strict);

        private readonly Mock<IQueryContext> queryContext = new Mock<IQueryContext>(MockBehavior.Strict);

        [TestMethod]
        public void WhenRegisteringWithNullServiceThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => QueryServiceExtensions.RegisterContextFactory<IQueryContext>(null, () => this.queryContext.Object), "queryService");
        }

        [TestMethod]
        public void WhenRegisteringContextFactoryThenRegistrationIsDelegated()
        {
            // Arrange
            this.queryService.Setup(s => s.RegisterContextFactory(It.IsAny<Type>(), It.IsAny<Func<IQueryContext>>()));

            // Act 
            QueryServiceExtensions.RegisterContextFactory<IQueryContext>(this.queryService.Object, () => this.queryContext.Object);

            // Assert
            this.queryService.Verify(s => s.RegisterContextFactory(It.IsAny<Type>(), It.IsAny<Func<IQueryContext>>()), Times.Once());
        }

        [TestMethod]
        public void WhenCreatingContextWithNullServiceThenThrowInvalidOperationException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => QueryServiceExtensions.CreateContext<IQueryContext>(null), "queryService");
        }

        [TestMethod]
        public void WhenCreatingContextThenCreationIsDelegated()
        {
            // Arrange
            this.queryService.Setup(s => s.CreateContext(It.IsAny<Type>())).Returns(this.queryContext.Object);

            // Act 
            IQueryContext result = QueryServiceExtensions.CreateContext<IQueryContext>(this.queryService.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, this.queryContext.Object.GetType());
            this.queryService.Verify(s => s.CreateContext(It.IsAny<Type>()), Times.Once());
        }
    }
}