namespace Waffle.Tests.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;

    [TestClass]
    public class DefaultCommandHandlerTypeResolverFixture
    {
        private readonly Mock<IAssembliesResolver> assembliesResolver = new Mock<IAssembliesResolver>(MockBehavior.Strict);

        [TestMethod]
        public void WhenCreatingHandlerTypesWithoutPredicateThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => new DefaultCommandHandlerTypeResolver(null), "predicate");
        }

        [TestMethod]
        public void WhenGettingHandlerTypesThenReturnsCollection()
        {
            // Assign
            DefaultCommandHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new Assembly[] { new ValidAssembly() });

            // Act
            var result = resolver.GetCommandHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(typeof(SimpleCommandHandler), result.First());
        }

        [TestMethod]
        public void WhenGettingHandlerTypesWithoutParametersThenThrowsArgumentNullException()
        {
            // Assign
            DefaultCommandHandlerTypeResolver resolver = this.CreateTestableService();

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => resolver.GetCommandHandlerTypes(null), "assembliesResolver");
        }

        [TestMethod]
        public void WhenGettingHandlerTypesFromDynamicAssemblyThenReturnsEmptyCollection()
        {
            // Assign
            DefaultCommandHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new Assembly[] { new DynamicAssembly() });

            // Act
            var result = resolver.GetCommandHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void WhenGettingHandlerTypesFromNullAssemblyThenReturnsEmptyCollection()
        {
            // Assign
            DefaultCommandHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new Assembly[] { null });

            // Act
            var result = resolver.GetCommandHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void WhenGettingHandlerTypesThrowReflectionTypeLoadExceptionThenReturnsCollection()
        {
            // Assign
            DefaultCommandHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new Assembly[] { new ThrowingReflectionTypeLoadExceptionAssembly() });

            // Act
            var result = resolver.GetCommandHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(typeof(SimpleCommandHandler), result.First());
        }

        [TestMethod]
        public void WhenGettingHandlerTypesThrowReflectionTypeLoadExceptionThenReturnsEmptyCollection()
        {
            // Assign
            DefaultCommandHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new Assembly[] { new ThrowingExceptionAssembly() });

            // Act
            var result = resolver.GetCommandHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        private static ICollection<Type> CreateTypeCollection()
        {
            return new[] 
            { 
                null,
                typeof(CommandHandler),
                typeof(AbstractCommandHandler),
                typeof(PrivateCommandHandler),
                typeof(ICommand),
                typeof(Command),
                typeof(SimpleCommandHandler)
            };
        }

        private DefaultCommandHandlerTypeResolver CreateTestableService()
        {
            return new DefaultCommandHandlerTypeResolver();
        }

        public abstract class AbstractCommandHandler : CommandHandler
        {
        }

        private class PrivateCommandHandler : CommandHandler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command, CommandHandlerContext context)
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