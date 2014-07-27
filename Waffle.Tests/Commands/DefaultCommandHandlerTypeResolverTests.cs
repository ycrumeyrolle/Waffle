namespace Waffle.Tests.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Tests.Helpers;
    using Xunit;

    public class DefaultCommandHandlerTypeResolverTests
    {
        private readonly Mock<IAssembliesResolver> assembliesResolver = new Mock<IAssembliesResolver>(MockBehavior.Strict);

        [Fact]
        public void WhenCreatingHandlerTypesWithoutPredicateThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => new DefaultCommandHandlerTypeResolver(null), "predicate");
        }

        [Fact]
        public void WhenGettingHandlerTypesThenReturnsCollection()
        {
            // Assign
            DefaultCommandHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new Assembly[] { new ValidAssembly() });

            // Act
            var result = resolver.GetCommandHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
            Assert.Equal(typeof(SimpleCommandHandler), result.First());
        }

        [Fact]
        public void WhenGettingHandlerTypesWithoutParametersThenThrowsArgumentNullException()
        {
            // Assign
            DefaultCommandHandlerTypeResolver resolver = this.CreateTestableService();

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => resolver.GetCommandHandlerTypes(null), "assembliesResolver");
        }

        [Fact]
        public void WhenGettingHandlerTypesFromDynamicAssemblyThenReturnsEmptyCollection()
        {
            // Assign
            DefaultCommandHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new Assembly[] { new DynamicAssembly() });

            // Act
            var result = resolver.GetCommandHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void WhenGettingHandlerTypesFromNullAssemblyThenReturnsEmptyCollection()
        {
            // Assign
            DefaultCommandHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new Assembly[] { null });

            // Act
            var result = resolver.GetCommandHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void WhenGettingHandlerTypesThrowReflectionTypeLoadExceptionThenReturnsCollection()
        {
            // Assign
            DefaultCommandHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new Assembly[] { new ThrowingReflectionTypeLoadExceptionAssembly() });

            // Act
            var result = resolver.GetCommandHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
            Assert.Equal(typeof(SimpleCommandHandler), result.First());
        }

        [Fact]
        public void WhenGettingHandlerTypesThrowReflectionTypeLoadExceptionThenReturnsEmptyCollection()
        {
            // Assign
            DefaultCommandHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new Assembly[] { new ThrowingExceptionAssembly() });

            // Act
            var result = resolver.GetCommandHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
        }

        private static IEnumerable<Type> CreateTypeCollection()
        {
            return new[] 
            { 
                null,
                typeof(MessageHandler),
                typeof(AbstractCommandHandler),
                typeof(PrivateCommandHandler),
                typeof(ICommand),
                typeof(SimpleCommandHandler)
            };
        }

        private DefaultCommandHandlerTypeResolver CreateTestableService()
        {
            return new DefaultCommandHandlerTypeResolver();
        }

        public abstract class AbstractCommandHandler : MessageHandler
        {
        }

        private class PrivateCommandHandler : MessageHandler, ICommandHandler<SimpleCommand>
        {
            public void Handle(SimpleCommand command)
            {
                throw new NotImplementedException();
            }
        }

        private class ValidAssembly : Assembly
        {
            public override Type[] GetTypes()
            {
                return CreateTypeCollection().ToArray();
            }
        }

        private class EmptyAssembly : Assembly
        {
            public override Type[] GetTypes()
            {
                return Type.EmptyTypes;
            }
        }

        private class ThrowingReflectionTypeLoadExceptionAssembly : Assembly
        {
            public override Type[] GetTypes()
            {
                throw new ReflectionTypeLoadException(CreateTypeCollection().ToArray(), null);
            }
        }

        private class ThrowingExceptionAssembly : Assembly
        {
            public override Type[] GetTypes()
            {
                throw new InvalidOperationException("YOUSHOULDNOTSEEME");
            }
        }

        private class DynamicAssembly : Assembly
        {
            public override bool IsDynamic
            {
                get
                {
                    return true;
                }
            }
        }
    }
}