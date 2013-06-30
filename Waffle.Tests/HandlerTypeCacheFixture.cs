namespace Waffle.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Waffle;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Dispatcher;
    using Waffle.Tests.Helpers;

    [TestClass]
    public sealed class HandlerTypeCacheFixture : IDisposable
    {
        private readonly ICollection<IDisposable> disposableResources = new Collection<IDisposable>();

        private readonly ProcessorConfiguration defaultConfig = new ProcessorConfiguration();

        private Mock<IHandlerTypeResolver> resolver = new Mock<IHandlerTypeResolver>(MockBehavior.Strict);

        public HandlerTypeCacheFixture()
        {
            this.disposableResources.Add(this.defaultConfig);
        }

        [TestMethod]
        public void WhenCreatingHandlerTypeCacheWithoutConfigThenThrowArgumentNullException()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new HandlerTypeCache(null), "configuration");
        }

        [TestMethod]
        public void WhenCreatingHandlerTypeCacheThenCacheIsReady()
        {
            // Arrange
            var types = CreateHandlerType();
            this.defaultConfig.Services.Replace(typeof(IHandlerTypeResolver), this.resolver.Object);
            this.resolver.Setup(r => r.GetHandlerTypes(It.IsAny<IAssembliesResolver>())).Returns(types);
            
            // Act
            HandlerTypeCache cache = new HandlerTypeCache(this.defaultConfig);
            
            // Assert
            Assert.IsNotNull(cache.Cache);
            Assert.AreEqual(typeof(SimpleCommand), cache.Cache.First().Key);
            Assert.AreEqual(1, cache.Cache.First().Value[typeof(SimpleHandler)].Count());
            Assert.AreEqual(typeof(SimpleHandler), cache.Cache.First().Value[typeof(SimpleHandler)].First());
        }

        [TestMethod]
        public void WhenGettingHandlerTypeFromCacheThenReturnTypes()
        {
            // Arrange
            var types = CreateHandlerType();
            this.defaultConfig.Services.Replace(typeof(IHandlerTypeResolver), this.resolver.Object);
            this.resolver.Setup(r => r.GetHandlerTypes(It.IsAny<IAssembliesResolver>())).Returns(types);
            HandlerTypeCache cache = new HandlerTypeCache(this.defaultConfig);

            // Act 
            var result = cache.GetHandlerTypes(typeof(SimpleCommand));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void WhenGettingUnknowHandlerTypeFromCacheThenReturnEmptyCollection()
        {
            // Arrange
            var types = CreateHandlerType();
            this.defaultConfig.Services.Replace(typeof(IHandlerTypeResolver), this.resolver.Object);
            this.resolver.Setup(r => r.GetHandlerTypes(It.IsAny<IAssembliesResolver>())).Returns(types);
            HandlerTypeCache cache = new HandlerTypeCache(this.defaultConfig);

            // Act 
            var result = cache.GetHandlerTypes(typeof(Command));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
        
        [TestMethod]
        public void WhenGttingNullHandlerTypeFromCacheThenThrowsArgumentNullException()
        {
            // Arrange
            var types = CreateHandlerType();
            this.defaultConfig.Services.Replace(typeof(IHandlerTypeResolver), this.resolver.Object);
            this.resolver.Setup(r => r.GetHandlerTypes(It.IsAny<IAssembliesResolver>())).Returns(types);
            HandlerTypeCache cache = new HandlerTypeCache(this.defaultConfig);

            // Act & aAssert
            ExceptionAssert.ThrowsArgumentNull(() => cache.GetHandlerTypes(null), "commandType");
        }

        private static Type[] CreateHandlerType()
        {
            return new[]
            {
              typeof(SimpleHandler),
              typeof(NoHandler)
            };
        }

        public class NoHandler : IComparable, IComparable<NoHandler>
        {
            public int CompareTo(NoHandler other)
            {
                throw new NotImplementedException();
            }

            public int CompareTo(object obj)
            {
                throw new NotImplementedException();
            }
        }
        
        [TestCleanup]
        public void Dispose()
        {
            this.defaultConfig.Dispose();
            foreach (IDisposable disposable in this.disposableResources)
            {
                disposable.Dispose();
            }
        }
    }
}