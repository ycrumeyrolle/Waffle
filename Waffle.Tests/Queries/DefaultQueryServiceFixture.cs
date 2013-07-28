namespace Waffle.Tests.Queries
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Queries;
    using Waffle.Tests.Helpers;

    [TestClass]
    public class DefaultQueryServiceFixture
    {
        private readonly Mock<IQueryContext> queryContext = new Mock<IQueryContext>(MockBehavior.Strict);

        [TestMethod]
        public void WhenRegisteringNullTypeThenThrowsArgumentNullException()
        {
            // Arrange
            var queryService = CreateService();

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => queryService.RegisterContextFactory(null, () => this.queryContext.Object), "contextType");
        }

        [TestMethod]
        public void WhenRegisteringNullContextFactoryThenThrowsArgumentNullException()
        {
            // Arrange
            var queryService = CreateService();

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => queryService.RegisterContextFactory<IQueryContext>(typeof(object), null), "queryContextFactory");
        }

        [TestMethod]
        public void WhenRegisteringContextFactoryThenFactoryIsRegistered()
        {
            // Arrange
            var queryService = CreateService();

            // Act & assert
            queryService.RegisterContextFactory(this.queryContext.Object.GetType(), () => this.queryContext.Object);
        }

        [TestMethod]
        public void WhenCreatingWitrhNullTypeThenThrowArgumentNullException()
        {
            // Arrange
            var queryService = CreateService();

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => queryService.CreateContext(null), "contextType");
        }

        [TestMethod]
        public void WhenCreatingUnknowContextThenThrowInvalidOperationException()
        {
            // Arrange
            var queryService = CreateService();

            // Act & assert
            ExceptionAssert.Throws<InvalidOperationException>(() => queryService.CreateContext(this.queryContext.Object.GetType()));
        }

        [TestMethod]
        public void WhenCreatingContextReturnNullThenThrowInvalidOperationException()
        {
            // Arrange
            var queryService = CreateService();
            queryService.RegisterContextFactory<IQueryContext>(this.queryContext.Object.GetType(), () => null);

            // Act & assert
            ExceptionAssert.Throws<InvalidOperationException>(() => queryService.CreateContext(this.queryContext.Object.GetType()));
        }

        [TestMethod]
        public void WhenCreatingContextThenReturnsContext()
        {
            // Arrange
            var queryService = CreateService();
            queryService.RegisterContextFactory(this.queryContext.Object.GetType(), () => this.queryContext.Object);

            // Act 
            IQueryContext result = queryService.CreateContext(this.queryContext.Object.GetType());

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, this.queryContext.Object.GetType());
        }

        private static DefaultQueryService CreateService()
        {
            DefaultQueryService queryService = new DefaultQueryService();
            return queryService;
        }
    }
}