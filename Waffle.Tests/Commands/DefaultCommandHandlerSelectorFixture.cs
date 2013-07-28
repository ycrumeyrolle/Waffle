namespace Waffle.Tests.Commands
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;

    [TestClass]
    public sealed class DefaultCommandHandlerSelectorFixture : IDisposable
    {
        private readonly ProcessorConfiguration config = new ProcessorConfiguration();

        [TestMethod]
        public void WhenCreatingDefaultHandlerSelectorWithourConfigurationThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => new DefaultCommandHandlerSelector(null), "configuration");
        }

        [TestMethod]
        public void WhenSelectingUnknownHandlerThenThrowsInvalidOperationException()
        {
            // Assign
            DefaultCommandHandlerSelector resolver = this.CreateTestableService();
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, new SimpleCommand());
            this.config.Services.Replace(typeof(ICommandHandlerTypeResolver), new EmptyCommandHandlerTypeResolver());

            // Act & assert
            ExceptionAssert.Throws<InvalidOperationException>(() => resolver.SelectHandler(request));
        }

        [TestMethod]
        public void WhenSelectingDuplicateHandlerThenThrowsInvalidOperationException()
        {
            // Assign
            DefaultCommandHandlerSelector resolver = this.CreateTestableService();
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, new SimpleCommand());
            this.config.Services.Replace(typeof(ICommandHandlerTypeResolver), new DuplicateCommandHandlerTypeResolver());

            // Act & assert
            ExceptionAssert.Throws<InvalidOperationException>(() => resolver.SelectHandler(request));
        }

        [TestMethod]
        public void WhenSelectingHandlerThenReturnHandlerDesciptor()
        {
            // Assign
            DefaultCommandHandlerSelector resolver = this.CreateTestableService();
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, new SimpleCommand());
            this.config.Services.Replace(typeof(ICommandHandlerTypeResolver), new SimpleCommandHandlerTypeResolver());

            // Act
            var descriptor = resolver.SelectHandler(request);
            
            // Assert
            Assert.IsNotNull(descriptor);
            Assert.AreEqual(typeof(SimpleHandler1), descriptor.HandlerType);
            Assert.AreEqual(typeof(SimpleCommand), descriptor.MessageType);
        }

        [TestMethod]
        public void WhenSelectingBadHandlerThenThrowsInvalidOperationException()
        {
            // Assign
            DefaultCommandHandlerSelector resolver = this.CreateTestableService();
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, new BadCommand());
            this.config.Services.Replace(typeof(ICommandHandlerTypeResolver), new BadCommandHandlerTypeResolver());

            // Act & assert
            ExceptionAssert.Throws<InvalidOperationException>(() => resolver.SelectHandler(request));
        }

        [TestMethod]
        public void WhenSelectingHandlerWithNullParamterThenThrowsArgumentNullException()
        {
            // Assign
            DefaultCommandHandlerSelector resolver = this.CreateTestableService();

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => resolver.SelectHandler(null), "request");
   
        }
        
        [TestMethod]
        public void WhenGettingHandlerMappingThenReturnsMapping()
        {
            // Assign
            DefaultCommandHandlerSelector resolver = this.CreateTestableService();
            this.config.Services.Replace(typeof(ICommandHandlerTypeResolver), new MultipleCommandHandlerTypeResolver());

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

        private DefaultCommandHandlerSelector CreateTestableService()
        {
            return new DefaultCommandHandlerSelector(this.config);
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

        private class SimpleHandler1 : CommandHandler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command, CommandHandlerContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class SimpleHandler2 : CommandHandler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command, CommandHandlerContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class SimpleHandler3 : CommandHandler<SimpleCommand2>
        {
            public override void Handle(SimpleCommand2 command, CommandHandlerContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class BadCommandHandler : CommandHandler<BadCommand, string>
        {
            public override string Handle(BadCommand command, CommandHandlerContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class EmptyCommandHandlerTypeResolver : ICommandHandlerTypeResolver
        {
            public ICollection<Type> GetCommandHandlerTypes(IAssembliesResolver assembliesResolver)
            {
                return Type.EmptyTypes;
            }
        }

        private class DuplicateCommandHandlerTypeResolver : ICommandHandlerTypeResolver
        {
            public ICollection<Type> GetCommandHandlerTypes(IAssembliesResolver assembliesResolver)
            {
                return new[] { typeof(SimpleHandler1), typeof(SimpleHandler2) };
            }
        }

        private class SimpleCommandHandlerTypeResolver : ICommandHandlerTypeResolver
        {
            public ICollection<Type> GetCommandHandlerTypes(IAssembliesResolver assembliesResolver)
            {
                return new[] { typeof(SimpleHandler1) };
            }
        }

        private class MultipleCommandHandlerTypeResolver : ICommandHandlerTypeResolver
        {
            public ICollection<Type> GetCommandHandlerTypes(IAssembliesResolver assembliesResolver)
            {
                return new[] { typeof(SimpleHandler1), typeof(SimpleHandler3) };
            }
        }

        private class BadCommandHandlerTypeResolver : ICommandHandlerTypeResolver
        {
            public ICollection<Type> GetCommandHandlerTypes(IAssembliesResolver assembliesResolver)
            {
                return new[] { typeof(BadCommandHandler) };
            }
        }

        public void Dispose()
        {
            this.config.Dispose();
        }
    }
}