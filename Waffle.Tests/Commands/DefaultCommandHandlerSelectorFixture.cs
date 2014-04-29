namespace Waffle.Tests.Commands
{
    using System;
    using System.Collections.Generic;
    using Xunit;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;
    using System.Threading.Tasks;

    
    public sealed class DefaultCommandHandlerSelectorFixture : IDisposable
    {
        private readonly ProcessorConfiguration config = new ProcessorConfiguration();

        [Fact]
        public void WhenCreatingDefaultHandlerSelectorWithourConfigurationThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => new DefaultCommandHandlerSelector(null), "configuration");
        }

        [Fact]
        public void WhenSelectingUnknownHandlerThenThrowsInvalidOperationException()
        {
            // Assign
            DefaultCommandHandlerSelector resolver = this.CreateTestableService();
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, new SimpleCommand());
            this.config.Services.Replace(typeof(ICommandHandlerTypeResolver), new EmptyCommandHandlerTypeResolver());

            // Act & assert
            Assert.Throws<InvalidOperationException>(() => resolver.SelectHandler(request));
        }

        [Fact]
        public void WhenSelectingDuplicateHandlerThenThrowsInvalidOperationException()
        {
            // Assign
            DefaultCommandHandlerSelector resolver = this.CreateTestableService();
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, new SimpleCommand());
            this.config.Services.Replace(typeof(ICommandHandlerTypeResolver), new DuplicateCommandHandlerTypeResolver());

            // Act & assert
            Assert.Throws<InvalidOperationException>(() => resolver.SelectHandler(request));
        }

        [Fact]
        public void WhenSelectingHandlerThenReturnHandlerDesciptor()
        {
            // Assign
            DefaultCommandHandlerSelector resolver = this.CreateTestableService();
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, new SimpleCommand());
            this.config.Services.Replace(typeof(ICommandHandlerTypeResolver), new SimpleCommandHandlerTypeResolver());

            // Act
            var descriptor = resolver.SelectHandler(request);
            
            // Assert
            Assert.NotNull(descriptor);
            Assert.Equal(typeof(SimpleHandler1), descriptor.HandlerType);
            Assert.Equal(typeof(SimpleCommand), descriptor.MessageType);
        }

        [Fact]
        public void WhenSelectingBadHandlerThenThrowsInvalidOperationException()
        {
            // Assign
            DefaultCommandHandlerSelector resolver = this.CreateTestableService();
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, new BadCommand());

            // Act & assert
            Assert.Throws<InvalidOperationException>(() => resolver.SelectHandler(request));
        }

        [Fact]
        public void WhenSelectingHandlerWithNullParamterThenThrowsArgumentNullException()
        {
            // Assign
            DefaultCommandHandlerSelector resolver = this.CreateTestableService();

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => resolver.SelectHandler(null), "request");
   
        }
        
        [Fact]
        public void WhenGettingHandlerMappingThenReturnsMapping()
        {
            // Assign
            DefaultCommandHandlerSelector resolver = this.CreateTestableService();
            this.config.Services.Replace(typeof(ICommandHandlerTypeResolver), new MultipleCommandHandlerTypeResolver());

            // Act
            var mapping = resolver.GetHandlerMapping();
            
            // Assert
            Assert.NotNull(mapping);
            Assert.Equal(2, mapping.Count);
            Assert.True(mapping.ContainsKey(typeof(SimpleCommand)));
            Assert.True(mapping.ContainsKey(typeof(SimpleCommand2)));

            Assert.Equal(mapping[typeof(SimpleCommand)].HandlerType, typeof(SimpleHandler1));
            Assert.Equal(mapping[typeof(SimpleCommand2)].HandlerType, typeof(SimpleHandler3));
        }

        private DefaultCommandHandlerSelector CreateTestableService()
        {
            return new DefaultCommandHandlerSelector(this.config);
        }

        private class SimpleCommand : ICommand
        {
        }

        private class SimpleCommand2 : ICommand
        {
        }

        private class BadCommand : ICommand
        {
        }

        private class SimpleHandler1 : MessageHandler, ICommandHandler<SimpleCommand>
        {
            public void Handle(SimpleCommand command)
            {
                throw new NotImplementedException();
            }
        }

        private class SimpleHandler2 : MessageHandler, ICommandHandler<SimpleCommand>
        {
            public void Handle(SimpleCommand command)
            {
                throw new NotImplementedException();
            }
        }

        private class SimpleHandler3 : MessageHandler, ICommandHandler<SimpleCommand2>
        {
            public void Handle(SimpleCommand2 command)
            {
                throw new NotImplementedException();
            }
        }

        private class BadCommandHandler : MessageHandler, ICommandHandler<BadCommand, string>
        {
            public string Handle(BadCommand command)
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