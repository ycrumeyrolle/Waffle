namespace CommandProcessing.Tests.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using CommandProcessing;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class DefaultHandlerSelectorFixture : IDisposable
    {
        private readonly ProcessorConfiguration config = new ProcessorConfiguration();

        [TestMethod]
        public void WhenCreatingDefaultHandlerSelectorWithourConfigurationThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => new DefaultHandlerSelector(null), "configuration");
        }

        [TestMethod]
        public void WhenSelectingUnknownHandlerThenThrowsInvalidOperationException()
        {
            // Assign
            DefaultHandlerSelector resolver = this.CreateTestableService();
            HandlerRequest request = new HandlerRequest(this.config, new SimpleCommand());
            this.config.Services.Replace(typeof(IHandlerTypeResolver), new EmptyHandlerTypeResolver());

            // Act & assert
            ExceptionAssert.Throws<InvalidOperationException>(() => resolver.SelectHandler(request));
        }

        [TestMethod]
        public void WhenSelectingDuplicateHandlerThenThrowsInvalidOperationException()
        {
            // Assign
            DefaultHandlerSelector resolver = this.CreateTestableService();
            HandlerRequest request = new HandlerRequest(this.config, new SimpleCommand());
            this.config.Services.Replace(typeof(IHandlerTypeResolver), new DuplicateHandlerTypeResolver());

            // Act & assert
            ExceptionAssert.Throws<InvalidOperationException>(() => resolver.SelectHandler(request));
        }

        [TestMethod]
        public void WhenSelectingHandlerThenReturnHandlerDesciptor()
        {
            // Assign
            DefaultHandlerSelector resolver = this.CreateTestableService();
            HandlerRequest request = new HandlerRequest(this.config, new SimpleCommand());
            this.config.Services.Replace(typeof(IHandlerTypeResolver), new SimpleHandlerTypeResolver());

            // Act
            var descriptor = resolver.SelectHandler(request);
            
            // Assert
            Assert.IsNotNull(descriptor);
            Assert.AreEqual(typeof(SimpleHandler1), descriptor.HandlerType);
            Assert.AreEqual(typeof(SimpleCommand), descriptor.CommandType);
        }

        [TestMethod]
        public void WhenSelectingBadHandlerThenThrowsInvalidOperationException()
        {
            // Assign
            DefaultHandlerSelector resolver = this.CreateTestableService();
            HandlerRequest request = new HandlerRequest(this.config, new BadCommand());
            this.config.Services.Replace(typeof(IHandlerTypeResolver), new BadHandlerTypeResolver());

            // Act & assert
            ExceptionAssert.Throws<InvalidOperationException>(() => resolver.SelectHandler(request));
        }

        [TestMethod]
        public void WhenSelectingHandlerWithNullParamterThenThrowsArgumentNullException()
        {
            // Assign
            DefaultHandlerSelector resolver = this.CreateTestableService();
            HandlerRequest request = null;

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => resolver.SelectHandler(request), "request");
   
        }
        
        [TestMethod]
        public void WhenGettingHandlerMappingThenReturnsMapping()
        {
            // Assign
            DefaultHandlerSelector resolver = this.CreateTestableService();
            this.config.Services.Replace(typeof(IHandlerTypeResolver), new MultipleHandlerTypeResolver());

            // Act
            var mapping = resolver.GetHandlerMapping();
            
            // Assert
            Assert.IsNotNull(mapping);
            Assert.AreEqual(2, mapping.Count);
            Assert.IsTrue(mapping.ContainsKey(typeof(SimpleCommand)));
            Assert.IsTrue(mapping.ContainsKey(typeof(SimpleCommand2)));

            Assert.AreEqual(mapping[typeof(SimpleCommand)].HandlerType, typeof(SimpleHandler1));
            Assert.AreEqual(mapping[typeof(SimpleCommand2)].HandlerType, typeof(SimpleHandler3));
        }

        private DefaultHandlerSelector CreateTestableService()
        {
            return new DefaultHandlerSelector(this.config);
        }

        private class SimpleCommand : Command
        {
        }

        private class SimpleCommand2 : Command
        {
        }

        private class BadCommand : Command
        {
        }

        private class SimpleHandler1 : Handler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command)
            {
                throw new NotImplementedException();
            }
        }

        private class SimpleHandler2 : Handler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command)
            {
                throw new NotImplementedException();
            }
        }

        private class SimpleHandler3 : Handler<SimpleCommand2>
        {
            public override void Handle(SimpleCommand2 command)
            {
                throw new NotImplementedException();
            }
        }

        private class BadHandler : Handler<BadCommand, string>
        {
            public override string Handle(BadCommand command)
            {
                throw new NotImplementedException();
            }
        }

        private class EmptyHandlerTypeResolver : IHandlerTypeResolver
        {
            public ICollection<Type> GetHandlerTypes(IAssembliesResolver assembliesResolver)
            {
                return Type.EmptyTypes;
            }
        }

        private class DuplicateHandlerTypeResolver : IHandlerTypeResolver
        {
            public ICollection<Type> GetHandlerTypes(IAssembliesResolver assembliesResolver)
            {
                return new[] { typeof(SimpleHandler1), typeof(SimpleHandler2) };
            }
        }

        private class SimpleHandlerTypeResolver : IHandlerTypeResolver
        {
            public ICollection<Type> GetHandlerTypes(IAssembliesResolver assembliesResolver)
            {
                return new[] { typeof(SimpleHandler1) };
            }
        }

        private class MultipleHandlerTypeResolver : IHandlerTypeResolver
        {
            public ICollection<Type> GetHandlerTypes(IAssembliesResolver assembliesResolver)
            {
                return new[] { typeof(SimpleHandler1), typeof(SimpleHandler3) };
            }
        }

        private class BadHandlerTypeResolver : IHandlerTypeResolver
        {
            public ICollection<Type> GetHandlerTypes(IAssembliesResolver assembliesResolver)
            {
                return new[] { typeof(BadHandler) };
            }
        }

        public void Dispose()
        {
            this.config.Dispose();
        }
    }
}