namespace Waffle.Tests.Caching
{
    using System;
    using System.Runtime.Caching;
    using System.Runtime.ExceptionServices;
    using System.Security.Principal;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Caching;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Tests.Commands;
    using Waffle.Tests.Helpers;

    [TestClass]
    public sealed class CacheAttributeFixture : IDisposable
    {
        private readonly Mock<ObjectCache> cache;

        private readonly ProcessorConfiguration config;

        public CacheAttributeFixture()
        {
            this.cache = new Mock<ObjectCache>();
            this.config = new ProcessorConfiguration();
        }

        [TestMethod]
        public void WhenExecutingFilterThenCacheIsChecked()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute();
            SimpleCommand command = new SimpleCommand { Property1 = 12, Property2 = "test" };
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, command);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerContext context = new CommandHandlerContext(request, descriptor);

            // Act
            filter.OnCommandExecuting(context);

            // Assert
            this.cache.Verify(c => c.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            Assert.IsNull(context.Response);
        }

        [TestMethod]
        public void WhenExecutingFilterWithResultInCacheThenCacheIsReturn()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute();
            SimpleCommand command = new SimpleCommand { Property1 = 12, Property2 = "test" };
            SimpleCommand cachedCommand = new SimpleCommand { Property1 = 12, Property2 = "test in cache" };
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, command);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerContext context = new CommandHandlerContext(request, descriptor);
            this.cache.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(new CacheAttribute.CacheEntry(cachedCommand));

            // Act
            filter.OnCommandExecuting(context);

            // Assert
            this.cache.Verify(c => c.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            Assert.IsNotNull(context.Response.Value);
            Assert.AreEqual(cachedCommand, context.Response.Value);
        }

        [TestMethod]
        public void WhenExecutingFilterWithoutContextThenThrowsArgumentNullException()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => filter.OnCommandExecuting(null), "CommandHandlerContext");
        }

        [TestMethod]
        public void WhenExecutingFilterToIgnoreThenCacheIsIgnored()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute();
            SimpleCommand command = new SimpleCommand { Property1 = 12, Property2 = "test" };
            SimpleCommand cachedCommand = new SimpleCommand { Property1 = 12, Property2 = "test in cache" };
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, command);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(NotCachedCommand), typeof(NotCachedCommandHandler));
            CommandHandlerContext context = new CommandHandlerContext(request, descriptor);
            this.cache.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(new CacheAttribute.CacheEntry(cachedCommand));

            // Act
            filter.OnCommandExecuting(context);

            // Assert
            this.cache.Verify(c => c.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            Assert.IsNull(context.Response);
        }

        [TestMethod]
        public void WhenExecutedFilterWithExceptionThenCacheIsNotUpdated()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute();
            SimpleCommand command = new SimpleCommand { Property1 = 12, Property2 = "test" };
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, command);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerContext context = new CommandHandlerContext(request, descriptor);
            Exception exception = new Exception();
            ExceptionDispatchInfo exceptionInfo = ExceptionDispatchInfo.Capture(exception);
            CommandHandlerExecutedContext executedContext = new CommandHandlerExecutedContext(context, exceptionInfo);

            // Act
            filter.OnCommandExecuted(executedContext);

            // Assert
            this.cache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()), Times.Never());
            Assert.IsNull(context.Response);
        }

        [TestMethod]
        public void WhenExecutedFilterWithoutContextThenThrowsArgumentNullException()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => filter.OnCommandExecuted(null), "handlerExecutedContext");
        }

        [TestMethod]
        public void WhenExecutedFilterToIgnoreThenCacheIsIgnored()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute();
            SimpleCommand command = new SimpleCommand { Property1 = 12, Property2 = "test" };
            SimpleCommand cachedCommand = new SimpleCommand { Property1 = 12, Property2 = "test in cache" };
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, command);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(NotCachedCommand), typeof(NotCachedCommandHandler));
            CommandHandlerContext context = new CommandHandlerContext(request, descriptor);
            CommandHandlerExecutedContext executedContext = new CommandHandlerExecutedContext(context, null);
            this.cache.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(new CacheAttribute.CacheEntry(cachedCommand));

            // Act
            filter.OnCommandExecuted(executedContext);

            // Assert
            this.cache.Verify(c => c.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            Assert.IsNull(context.Response);
        }

        [TestMethod]
        public void WhenExecutedFilterWithoutKeyThenCacheIsNotUpdated()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute();
            SimpleCommand command = new SimpleCommand { Property1 = 12, Property2 = "test" };
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, command);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerContext context = new CommandHandlerContext(request, descriptor);

            CommandHandlerExecutedContext executedContext = new CommandHandlerExecutedContext(context, null);

            // Act
            filter.OnCommandExecuted(executedContext);

            // Assert
            this.cache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void WhenExecutedFilterWithKeyEmptyThenCacheIsNotUpdated()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute();
            SimpleCommand command = new SimpleCommand { Property1 = 12, Property2 = "test" };
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, command);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerContext context = new CommandHandlerContext(request, descriptor);
            filter.OnCommandExecuting(context);
            context.Items["__CacheAttribute"] = null;
            CommandHandlerExecutedContext executedContext = new CommandHandlerExecutedContext(context, null);

            // Act
            filter.OnCommandExecuted(executedContext);

            // Assert
            this.cache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void WhenExecutedFilterWithKeyThenCacheIsUpdated()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute();
            SimpleCommand command = new SimpleCommand { Property1 = 12, Property2 = "test" };
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, command);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerContext context = new CommandHandlerContext(request, descriptor);
            filter.OnCommandExecuting(context);

            CommandHandlerExecutedContext executedContext = new CommandHandlerExecutedContext(context, null);

            // Act
            filter.OnCommandExecuted(executedContext);

            // Assert
            this.cache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void WhenExecutedFilterVaryByParamsThenCacheIsUpdated()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute(new MemoryCache("test"));
            filter.Duration = 10;
            filter.VaryByParams = "Property1";

            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            SimpleCommand command1 = new SimpleCommand { Property1 = 1, Property2 = "test1" };
            CommandHandlerRequest request1 = new CommandHandlerRequest(this.config, command1);
            CommandHandlerContext context1 = new CommandHandlerContext(request1, descriptor);
            CommandHandlerExecutedContext executedContext1 = new CommandHandlerExecutedContext(context1, null);
            executedContext1.Response = request1.CreateResponse("result1");

            SimpleCommand command2 = new SimpleCommand { Property1 = 2, Property2 = "test2" };
            CommandHandlerRequest request2 = new CommandHandlerRequest(this.config, command2);
            CommandHandlerContext context2 = new CommandHandlerContext(request2, descriptor);
            CommandHandlerExecutedContext executedContext2 = new CommandHandlerExecutedContext(context2, null);
            executedContext2.Response = request2.CreateResponse("result2");

            SimpleCommand command3 = new SimpleCommand { Property1 = 2, Property2 = "test3" };
            CommandHandlerRequest request3 = new CommandHandlerRequest(this.config, command3);
            CommandHandlerContext context3 = new CommandHandlerContext(request3, descriptor);
            CommandHandlerExecutedContext executedContext3 = new CommandHandlerExecutedContext(context3, null);
            executedContext3.Response = request3.CreateResponse("result3");

            // Act
            filter.OnCommandExecuting(context1);
            filter.OnCommandExecuted(executedContext1);
            filter.OnCommandExecuting(context2);
            filter.OnCommandExecuted(executedContext2);
            filter.OnCommandExecuting(context3);
            filter.OnCommandExecuted(executedContext3);

            // Assert
            Assert.AreEqual("result1", executedContext1.Response.Value);
            Assert.AreEqual("result2", executedContext2.Response.Value);
            Assert.AreEqual("result2", executedContext3.Response.Value);
        }

        [TestMethod]
        public void WhenExecutedFilterVaryByParamsSetToNoneThenCacheIsAlwaysUsed()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute(new MemoryCache("test"));
            filter.Duration = 10;
            filter.VaryByParams = "none";

            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand2), typeof(SimpleCommandHandler2));
            SimpleCommand command1 = new SimpleCommand { Property1 = 1, Property2 = "test1" };
            CommandHandlerRequest request1 = new CommandHandlerRequest(this.config, command1);
            CommandHandlerContext context1 = new CommandHandlerContext(request1, descriptor);
            CommandHandlerExecutedContext executedContext1 = new CommandHandlerExecutedContext(context1, null);
            executedContext1.SetResponse("result1");

            SimpleCommand command2 = new SimpleCommand { Property1 = 2, Property2 = "test2" };
            CommandHandlerRequest request2 = new CommandHandlerRequest(this.config, command2);
            CommandHandlerContext context2 = new CommandHandlerContext(request2, descriptor);
            CommandHandlerExecutedContext executedContext2 = new CommandHandlerExecutedContext(context2, null);
            executedContext2.SetResponse("result2");

            SimpleCommand command3 = new SimpleCommand { Property1 = 2, Property2 = "test3" };
            CommandHandlerRequest request3 = new CommandHandlerRequest(this.config, command3);
            CommandHandlerContext context3 = new CommandHandlerContext(request3, descriptor);
            CommandHandlerExecutedContext executedContext3 = new CommandHandlerExecutedContext(context3, null);
            executedContext3.SetResponse( "result3");

            // Act
            filter.OnCommandExecuting(context1);
            filter.OnCommandExecuted(executedContext1);
            filter.OnCommandExecuting(context2);
            filter.OnCommandExecuted(executedContext2);
            filter.OnCommandExecuting(context3);
            filter.OnCommandExecuted(executedContext3);

            // Assert
            Assert.AreEqual("result1", executedContext1.Response.Value);
            Assert.AreEqual("result1", executedContext2.Response.Value);
            Assert.AreEqual("result1", executedContext3.Response.Value);
        }

        [TestMethod]
        public void WhenExecutedFilterVaryByParamsSetIncorrectlyThenCacheIsAlwaysUsed()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute(new MemoryCache("test"));
            filter.Duration = 10;
            filter.VaryByParams = "XXXX";

            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            SimpleCommand command1 = new SimpleCommand { Property1 = 1, Property2 = "test1" };
            CommandHandlerRequest request1 = new CommandHandlerRequest(this.config, command1);
            CommandHandlerContext context1 = new CommandHandlerContext(request1, descriptor);
            CommandHandlerExecutedContext executedContext1 = new CommandHandlerExecutedContext(context1, null);
            executedContext1.SetResponse("result1");

            SimpleCommand command2 = new SimpleCommand { Property1 = 2, Property2 = "test2" };
            CommandHandlerRequest request2 = new CommandHandlerRequest(this.config, command2);
            CommandHandlerContext context2 = new CommandHandlerContext(request2, descriptor);
            CommandHandlerExecutedContext executedContext2 = new CommandHandlerExecutedContext(context2, null);
            executedContext2.SetResponse( "result2");

            SimpleCommand command3 = new SimpleCommand { Property1 = 2, Property2 = "test3" };
            CommandHandlerRequest request3 = new CommandHandlerRequest(this.config, command3);
            CommandHandlerContext context3 = new CommandHandlerContext(request3, descriptor);
            CommandHandlerExecutedContext executedContext3 = new CommandHandlerExecutedContext(context3, null);
            executedContext3.SetResponse("result3");

            // Act
            filter.OnCommandExecuting(context1);
            filter.OnCommandExecuted(executedContext1);
            filter.OnCommandExecuting(context2);
            filter.OnCommandExecuted(executedContext2);
            filter.OnCommandExecuting(context3);
            filter.OnCommandExecuted(executedContext3);

            // Assert
            Assert.AreEqual("result1", executedContext1.Response.Value);
            Assert.AreEqual("result1", executedContext2.Response.Value);
            Assert.AreEqual("result1", executedContext3.Response.Value);
        }

        [TestMethod]
        public void WhenSettingNullToVaryByParamsThenThrowsArgumentNullException()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute();

            // Act & assert
            ExceptionAssert.Throws<ArgumentNullException>(() => filter.VaryByParams = null);
        }

        [TestMethod]
        public void WhenExecutedFilterVaryByUserThenCacheIsUpdated()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute(new MemoryCache("test"));
            filter.Duration = 10;
            filter.VaryByUser = true;
            filter.VaryByParams = CacheAttribute.VaryByParamsNone;

            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            SimpleCommand command1 = new SimpleCommand { Property1 = 1, Property2 = "test1" };
            CommandHandlerRequest request1 = new CommandHandlerRequest(this.config, command1);
            CommandHandlerContext context1 = new CommandHandlerContext(request1, descriptor);
            context1.User = new GenericPrincipal(new GenericIdentity("user1"), null);
            CommandHandlerExecutedContext executedContext1 = new CommandHandlerExecutedContext(context1, null);
            executedContext1.Response = new HandlerResponse(request1, "result1");

            SimpleCommand command2 = new SimpleCommand { Property1 = 1, Property2 = "test1" };
            CommandHandlerRequest request2 = new CommandHandlerRequest(this.config, command2);
            CommandHandlerContext context2 = new CommandHandlerContext(request2, descriptor);
            context2.User = new GenericPrincipal(new GenericIdentity("user2"), null);
            CommandHandlerExecutedContext executedContext2 = new CommandHandlerExecutedContext(context2, null);
            executedContext2.Response = new HandlerResponse(request2, "result2");

            SimpleCommand command3 = new SimpleCommand { Property1 = 1, Property2 = "test1" };
            CommandHandlerRequest request3 = new CommandHandlerRequest(this.config, command3);
            CommandHandlerContext context3 = new CommandHandlerContext(request3, descriptor);
            context3.User = new GenericPrincipal(new GenericIdentity("user1"), null);
            CommandHandlerExecutedContext executedContext3 = new CommandHandlerExecutedContext(context3, null);
            executedContext3.Response = new HandlerResponse(request3, "result3");

            // Act
            filter.OnCommandExecuting(context1);
            filter.OnCommandExecuted(executedContext1);
            filter.OnCommandExecuting(context2);
            filter.OnCommandExecuted(executedContext2);
            filter.OnCommandExecuting(context3);
            filter.OnCommandExecuted(executedContext3);

            // Assert
            Assert.AreEqual("result1", executedContext1.Response.Value);
            Assert.AreEqual("result2", executedContext2.Response.Value);
            Assert.AreEqual("result1", executedContext3.Response.Value);
        }

        private CacheAttribute CreateAttribute(ObjectCache innerCache = null)
        {
            return new CacheAttribute(innerCache ?? this.cache.Object);
        }

        public void Dispose()
        {
            this.config.Dispose();
        }
    }
}
