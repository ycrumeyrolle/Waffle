namespace Waffle.Tests.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Moq;
    using Waffle.Commands;
    using Waffle.Tests.Helpers;
    using Xunit;

    public sealed class CommandHandlerTypeCacheTests : IDisposable
    {
        private readonly ICollection<IDisposable> disposableResources = new Collection<IDisposable>();

        private readonly ProcessorConfiguration defaultConfig = new ProcessorConfiguration();

        private readonly Mock<ICommandHandlerTypeResolver> resolver = new Mock<ICommandHandlerTypeResolver>(MockBehavior.Strict);

        public CommandHandlerTypeCacheTests()
        {
            this.disposableResources.Add(this.defaultConfig);
        }

        [Fact]
        public void WhenCreatingHandlerTypeCacheWithoutConfigThenThrowArgumentNullException()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new CommandHandlerTypeCache(null), "configuration");
        }

        [Fact]
        public void WhenCreatingHandlerTypeCacheThenCacheIsReady()
        {
            // Arrange
            var types = CreateHandlerType();
            this.defaultConfig.Services.Replace(typeof(ICommandHandlerTypeResolver), this.resolver.Object);
            this.resolver.Setup(r => r.GetCommandHandlerTypes(It.IsAny<IAssembliesResolver>())).Returns(types);
            
            // Act
            CommandHandlerTypeCache cache = new CommandHandlerTypeCache(this.defaultConfig);
            
            // Assert
            Assert.NotNull(cache.Cache);
            Assert.Equal(typeof(SimpleCommand), cache.Cache.First().Key);
            Assert.Equal(1, cache.Cache.First().Value[typeof(SimpleCommandHandler)].Count());
            Assert.Equal(typeof(SimpleCommandHandler), cache.Cache.First().Value[typeof(SimpleCommandHandler)].First());
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
        }
        
        [Fact]
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