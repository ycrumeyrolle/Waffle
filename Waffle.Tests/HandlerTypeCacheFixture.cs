namespace Waffle.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Tests.Commands;
    using Waffle.Tests.Helpers;

    [TestClass]
    public sealed class HandlerTypeCacheFixture : IDisposable
    {
        private readonly ICollection<IDisposable> disposableResources = new Collection<IDisposable>();

        private readonly ProcessorConfiguration defaultConfig = new ProcessorConfiguration();

        private Mock<ICommandHandlerTypeResolver> resolver = new Mock<ICommandHandlerTypeResolver>(MockBehavior.Strict);

        public HandlerTypeCacheFixture()
        {
            this.disposableResources.Add(this.defaultConfig);
        }

        [TestMethod]
        public void WhenCreatingHandlerTypeCacheWithoutConfigThenThrowArgumentNullException()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new CommandHandlerTypeCache(null), "configuration");
        }

        [TestMethod]
        public void WhenCreatingHandlerTypeCacheThenCacheIsReady()
        {
            // Arrange
            var types = CreateHandlerType();
            this.defaultConfig.Services.Replace(typeof(ICommandHandlerTypeResolver), this.resolver.Object);
            this.resolver.Setup(r => r.GetCommandHandlerTypes(It.IsAny<IAssembliesResolver>())).Returns(types);
            
            // Act
            CommandHandlerTypeCache cache = new CommandHandlerTypeCache(this.defaultConfig);
            
            // Assert
            Assert.IsNotNull(cache.Cache);
            Assert.AreEqual(typeof(SimpleCommand), cache.Cache.First().Key);
            Assert.AreEqual(1, cache.Cache.First().Value[typeof(SimpleCommandHandler)].Count());
            Assert.AreEqual(typeof(SimpleCommandHandler), cache.Cache.First().Value[typeof(SimpleCommandHandler)].First());
        }

        [TestMethod]
        public void WhenGettingHandlerTypeFromCacheThenReturnTypes()
        {
            // Arrange
            var types = CreateHandlerType();
            this.defaultConfig.Services.Replace(typeof(ICommandHandlerTypeResolver), this.resolver.Object);
            this.resolver.Setup(r => r.GetCommandHandlerTypes(It.IsAny<IAssembliesResolver>())).Returns(types);
            CommandHandlerTypeCache cache = new CommandHandlerTypeCache(this.defaultConfig);

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
            this.defaultConfig.Services.Replace(typeof(ICommandHandlerTypeResolver), this.resolver.Object);
            this.resolver.Setup(r => r.GetCommandHandlerTypes(It.IsAny<IAssembliesResolver>())).Returns(types);
            CommandHandlerTypeCache cache = new CommandHandlerTypeCache(this.defaultConfig);

            // Act 
            var result = cache.GetHandlerTypes(typeof(ICommand));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
        
        [TestMethod]
        public void WhenGttingNullHandlerTypeFromCacheThenThrowsArgumentNullException()
        {
            // Arrange
            var types = CreateHandlerType();
            this.defaultConfig.Services.Replace(typeof(ICommandHandlerTypeResolver), this.resolver.Object);
            this.resolver.Setup(r => r.GetCommandHandlerTypes(It.IsAny<IAssembliesResolver>())).Returns(types);
            CommandHandlerTypeCache cache = new CommandHandlerTypeCache(this.defaultConfig);

            // Act & aAssert
            ExceptionAssert.ThrowsArgumentNull(() => cache.GetHandlerTypes(null), "commandType");
        }

        private static Type[] CreateHandlerType()
        {
            return new[]
            {
              typeof(SimpleCommandHandler),
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