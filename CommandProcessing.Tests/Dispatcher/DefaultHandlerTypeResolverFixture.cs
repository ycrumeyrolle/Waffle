namespace CommandProcessing.Tests.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CommandProcessing;
    using CommandProcessing.Dispatcher;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    [TestClass]
    public class DefaultHandlerTypeResolverFixture
    {
        private readonly Mock<IAssembliesResolver> assembliesResolver = new Mock<IAssembliesResolver>(MockBehavior.Strict);
        
        [TestMethod]
        public void WhenGettingHandlerTypesThenReturnsCollection()
        {
            // Assign
            DefaultHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new[] { new ValidAssembly() });

            // Act
            var result = resolver.GetHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(typeof(SimpleHandler), result.First());
        }

        [TestMethod]
        public void WhenGettingHandlerTypesWithoutParametersThenThrowsArgumentNullException()
        {
            // Assign
            DefaultHandlerTypeResolver resolver = this.CreateTestableService();
            bool exceptionRaised = false;

            // Act
            try
            {
                resolver.GetHandlerTypes(null);
            }
            catch (ArgumentNullException)
            {
                exceptionRaised = true;
            }

            // Assert
            Assert.IsTrue(exceptionRaised);
        }

        [TestMethod]
        public void WhenGettingHandlerTypesFromDynamicAssemblyThenReturnsEmptyCollection()
        {
            // Assign
            DefaultHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new[] { new DynamicAssembly() });

            // Act
            var result = resolver.GetHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void WhenGettingHandlerTypesFromNullAssemblyThenReturnsEmptyCollection()
        {
            // Assign
            DefaultHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new Assembly[] { null });

            // Act
            var result = resolver.GetHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void WhenGettingHandlerTypesThrowReflectionTypeLoadExceptionThenReturnsCollection()
        {
            // Assign
            DefaultHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new[] { new ThrowingReflectionTypeLoadExceptionAssembly() });

            // Act
            var result = resolver.GetHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(typeof(SimpleHandler), result.First());
        }

        [TestMethod]
        public void WhenGettingHandlerTypesThrowReflectionTypeLoadExceptionThenReturnsEmptyCollection()
        {
            // Assign
            DefaultHandlerTypeResolver resolver = this.CreateTestableService();
            this.assembliesResolver.Setup(r => r.GetAssemblies()).Returns(new[] { new ThrowingExceptionAssembly() });

            // Act
            var result = resolver.GetHandlerTypes(this.assembliesResolver.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        private static ICollection<Type> CreateTypeCollection()
        {
            return new[] 
            { 
                null,
                typeof(Handler),
                typeof(AbstractHandler),
                typeof(PrivateHandler),
                typeof(ICommand),
                typeof(Command),
                typeof(SimpleHandler),
            };
        }

        private DefaultHandlerTypeResolver CreateTestableService()
        {
            return new DefaultHandlerTypeResolver();
        }

        public abstract class AbstractHandler : Handler
        {
        }

        private class PrivateHandler : Handler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command)
            {
                throw new NotImplementedException();
            }
        }

        private class ValidAssembly : Assembly
        {
            public override Type[] GetExportedTypes()
            {
                return CreateTypeCollection().ToArray();
            }
        }

        private class EmptyAssembly : Assembly
        {
            public override Type[] GetExportedTypes()
            {
                return Type.EmptyTypes;
            }
        }

        private class ThrowingReflectionTypeLoadExceptionAssembly : Assembly
        {
            public override Type[] GetExportedTypes()
            {
                throw new ReflectionTypeLoadException(CreateTypeCollection().ToArray(), null);
            }
        }

        private class ThrowingExceptionAssembly : Assembly
        {
            public override Type[] GetExportedTypes()
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