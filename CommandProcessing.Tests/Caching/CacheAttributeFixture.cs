namespace CommandProcessing.Tests.Caching
{
    using System;
    using System.Runtime.Caching;
    using System.Security.Principal;
    using CommandProcessing.Caching;
    using CommandProcessing.Filters;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CacheAttributeFixture
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
            filter.Duration = 1;
            SimpleCommand command = new SimpleCommand { Property1 = 12, Property2 = "test" };
            HandlerRequest request = new HandlerRequest(this.config, command);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            HandlerContext context = new HandlerContext(request, descriptor);

            // Act
            filter.OnCommandExecuting(context);

            // Assert
            this.cache.Verify(c => c.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            Assert.IsNull(context.Result);
        }

        [TestMethod]
        public void WhenExecutingFilterWithResultInCacheThenCacheIsReturn()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute();
            filter.Duration = 1;
            SimpleCommand command = new SimpleCommand { Property1 = 12, Property2 = "test" };
            SimpleCommand cachedCommand = new SimpleCommand { Property1 = 12, Property2 = "test in cache" };
            HandlerRequest request = new HandlerRequest(this.config, command);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            HandlerContext context = new HandlerContext(request, descriptor);
            this.cache.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(new CacheAttribute.CacheEntry(cachedCommand));

            // Act
            filter.OnCommandExecuting(context);

            // Assert
            this.cache.Verify(c => c.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            Assert.IsNotNull(context.Result);
            Assert.AreEqual(cachedCommand, context.Result);
        }

        [TestMethod]
        public void WhenExecutedFilterWithExceptionThenCacheIsNotUpdated()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute();
            filter.Duration = 1;
            SimpleCommand command = new SimpleCommand { Property1 = 12, Property2 = "test" };
            HandlerRequest request = new HandlerRequest(this.config, command);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            HandlerContext context = new HandlerContext(request, descriptor);
            HandlerExecutedContext executedContext = new HandlerExecutedContext(context, new Exception());

            // Act
            filter.OnCommandExecuted(executedContext);

            // Assert
            this.cache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()), Times.Never());
            Assert.IsNull(context.Result);
        }

        [TestMethod]
        public void WhenExecutedFilterWithoutKeyThenCacheIsNotUpdated()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute();
            filter.Duration = 1;
            SimpleCommand command = new SimpleCommand { Property1 = 12, Property2 = "test" };
            HandlerRequest request = new HandlerRequest(this.config, command);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            HandlerContext context = new HandlerContext(request, descriptor);

            HandlerExecutedContext executedContext = new HandlerExecutedContext(context, null);

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
            filter.Duration = 1;
            SimpleCommand command = new SimpleCommand { Property1 = 12, Property2 = "test" };
            HandlerRequest request = new HandlerRequest(this.config, command);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            HandlerContext context = new HandlerContext(request, descriptor);
            filter.OnCommandExecuting(context);
            context.Items["__CacheAttribute"] = null;
            HandlerExecutedContext executedContext = new HandlerExecutedContext(context, null);

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
            filter.Duration = 1;
            SimpleCommand command = new SimpleCommand { Property1 = 12, Property2 = "test" };
            HandlerRequest request = new HandlerRequest(this.config, command);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            HandlerContext context = new HandlerContext(request, descriptor);
            filter.OnCommandExecuting(context);

            HandlerExecutedContext executedContext = new HandlerExecutedContext(context, null);

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

            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            SimpleCommand command1 = new SimpleCommand { Property1 = 1, Property2 = "test1" };
            HandlerRequest request1 = new HandlerRequest(this.config, command1);
            HandlerContext context1 = new HandlerContext(request1, descriptor);
            HandlerExecutedContext executedContext1 = new HandlerExecutedContext(context1, null);
            executedContext1.Result = "result1";

            SimpleCommand command2 = new SimpleCommand { Property1 = 2, Property2 = "test2" };
            HandlerRequest request2 = new HandlerRequest(this.config, command2);
            HandlerContext context2 = new HandlerContext(request2, descriptor);
            HandlerExecutedContext executedContext2 = new HandlerExecutedContext(context2, null);
            executedContext2.Result = "result2";

            SimpleCommand command3 = new SimpleCommand { Property1 = 2, Property2 = "test3" };
            HandlerRequest request3 = new HandlerRequest(this.config, command3);
            HandlerContext context3 = new HandlerContext(request3, descriptor);
            HandlerExecutedContext executedContext3 = new HandlerExecutedContext(context3, null);
            executedContext3.Result = "result3";

            // Act
            filter.OnCommandExecuting(context1);
            filter.OnCommandExecuted(executedContext1);
            filter.OnCommandExecuting(context2);
            filter.OnCommandExecuted(executedContext2);
            filter.OnCommandExecuting(context3);
            filter.OnCommandExecuted(executedContext3);

            // Assert
            Assert.AreEqual("result1", executedContext1.Result);
            Assert.AreEqual("result2", executedContext2.Result);
            Assert.AreEqual("result2", executedContext3.Result);
        }

        [TestMethod]
        public void WhenExecutedFilterVaryByParamsSetToNoneThenCacheIsAlwaysUsed()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute(new MemoryCache("test"));
            filter.Duration = 10;
            filter.VaryByParams = "none";

            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            SimpleCommand command1 = new SimpleCommand { Property1 = 1, Property2 = "test1" };
            HandlerRequest request1 = new HandlerRequest(this.config, command1);
            HandlerContext context1 = new HandlerContext(request1, descriptor);
            HandlerExecutedContext executedContext1 = new HandlerExecutedContext(context1, null);
            executedContext1.Result = "result1";

            SimpleCommand command2 = new SimpleCommand { Property1 = 2, Property2 = "test2" };
            HandlerRequest request2 = new HandlerRequest(this.config, command2);
            HandlerContext context2 = new HandlerContext(request2, descriptor);
            HandlerExecutedContext executedContext2 = new HandlerExecutedContext(context2, null);
            executedContext2.Result = "result2";

            SimpleCommand command3 = new SimpleCommand { Property1 = 2, Property2 = "test3" };
            HandlerRequest request3 = new HandlerRequest(this.config, command3);
            HandlerContext context3 = new HandlerContext(request3, descriptor);
            HandlerExecutedContext executedContext3 = new HandlerExecutedContext(context3, null);
            executedContext3.Result = "result3";

            // Act
            filter.OnCommandExecuting(context1);
            filter.OnCommandExecuted(executedContext1);
            filter.OnCommandExecuting(context2);
            filter.OnCommandExecuted(executedContext2);
            filter.OnCommandExecuting(context3);
            filter.OnCommandExecuted(executedContext3);

            // Assert
            Assert.AreEqual("result1", executedContext1.Result);
            Assert.AreEqual("result1", executedContext2.Result);
            Assert.AreEqual("result1", executedContext3.Result);
        }

        [TestMethod]
        public void WhenExecutedFilterVaryByParamsSetIncorrectlyThenCacheIsAlwaysUsed()
        {
            // Arrange
            CacheAttribute filter = this.CreateAttribute(new MemoryCache("test"));
            filter.Duration = 10;
            filter.VaryByParams = "XXXX";

            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            SimpleCommand command1 = new SimpleCommand { Property1 = 1, Property2 = "test1" };
            HandlerRequest request1 = new HandlerRequest(this.config, command1);
            HandlerContext context1 = new HandlerContext(request1, descriptor);
            HandlerExecutedContext executedContext1 = new HandlerExecutedContext(context1, null);
            executedContext1.Result = "result1";

            SimpleCommand command2 = new SimpleCommand { Property1 = 2, Property2 = "test2" };
            HandlerRequest request2 = new HandlerRequest(this.config, command2);
            HandlerContext context2 = new HandlerContext(request2, descriptor);
            HandlerExecutedContext executedContext2 = new HandlerExecutedContext(context2, null);
            executedContext2.Result = "result2";

            SimpleCommand command3 = new SimpleCommand { Property1 = 2, Property2 = "test3" };
            HandlerRequest request3 = new HandlerRequest(this.config, command3);
            HandlerContext context3 = new HandlerContext(request3, descriptor);
            HandlerExecutedContext executedContext3 = new HandlerExecutedContext(context3, null);
            executedContext3.Result = "result3";

            // Act
            filter.OnCommandExecuting(context1);
            filter.OnCommandExecuted(executedContext1);
            filter.OnCommandExecuting(context2);
            filter.OnCommandExecuted(executedContext2);
            filter.OnCommandExecuting(context3);
            filter.OnCommandExecuted(executedContext3);

            // Assert
            Assert.AreEqual("result1", executedContext1.Result);
            Assert.AreEqual("result1", executedContext2.Result);
            Assert.AreEqual("result1", executedContext3.Result);
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

            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            SimpleCommand command1 = new SimpleCommand { Property1 = 1, Property2 = "test1" };
            HandlerRequest request1 = new HandlerRequest(this.config, command1);
            HandlerContext context1 = new HandlerContext(request1, descriptor);
            context1.User = new GenericPrincipal(new GenericIdentity("user1"), null);
            HandlerExecutedContext executedContext1 = new HandlerExecutedContext(context1, null);
            executedContext1.Result = "result1";

            SimpleCommand command2 = new SimpleCommand { Property1 = 1, Property2 = "test1" };
            HandlerRequest request2 = new HandlerRequest(this.config, command2);
            HandlerContext context2 = new HandlerContext(request2, descriptor);
            context2.User = new GenericPrincipal(new GenericIdentity("user2"), null);
            HandlerExecutedContext executedContext2 = new HandlerExecutedContext(context2, null);
            executedContext2.Result = "result2";

            SimpleCommand command3 = new SimpleCommand { Property1 = 1, Property2 = "test1" };
            HandlerRequest request3 = new HandlerRequest(this.config, command3);
            HandlerContext context3 = new HandlerContext(request3, descriptor);
            context3.User = new GenericPrincipal(new GenericIdentity("user1"), null);
            HandlerExecutedContext executedContext3 = new HandlerExecutedContext(context3, null);
            executedContext3.Result = "result3";

            // Act
            filter.OnCommandExecuting(context1);
            filter.OnCommandExecuted(executedContext1);
            filter.OnCommandExecuting(context2);
            filter.OnCommandExecuted(executedContext2);
            filter.OnCommandExecuting(context3);
            filter.OnCommandExecuted(executedContext3);

            // Assert
            Assert.AreEqual("result1", executedContext1.Result);
            Assert.AreEqual("result2", executedContext2.Result);
            Assert.AreEqual("result1", executedContext3.Result);
        }

        private CacheAttribute CreateAttribute(ObjectCache innerCache = null)
        {
            return new CacheAttribute(innerCache ?? this.cache.Object);
        }
    }
}
