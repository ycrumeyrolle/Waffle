namespace Waffle.Unity.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Practices.Unity;
    using Moq;
    using Waffle;
    using Waffle.Commands;
    using Xunit;

    public sealed class CommandProcessorWithUnityTests : IDisposable
    {
        private readonly ICollection<IDisposable> disposableResources = new Collection<IDisposable>();

        private readonly ProcessorConfiguration configuration = new ProcessorConfiguration();
        
        private readonly IUnityContainer container = new UnityContainer();

        private readonly Mock<ICommandHandlerTypeResolver> resolver = new Mock<ICommandHandlerTypeResolver>();

        [Fact]
        public async void WhenProcessingValidCommandThenCommandIsProcessed()
        {
            // Arrange
            this.resolver
                .Setup(r => r.GetCommandHandlerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new[] { typeof(ValidCommandHandler) });
            this.configuration.Services.Replace(typeof(ICommandHandlerTypeResolver), this.resolver.Object);
            var service = new Mock<ISimpleService>();

            this.container.RegisterInstance(service.Object);
            MessageProcessor processor = this.CreateTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            await processor.ProcessAsync(command);

            // Assert
            service.Verify(s => s.Execute(), Times.Once());
        }
        
#if LOOSE_CQRS
        [Fact]
        public async Task WhenProcessingCommandWithoutResultThenCommandIsProcessed()
        {
            // Arrange
            this.resolver
                .Setup(r => r.GetCommandHandlerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new[] { typeof(ValidCommandHandlerWithResult) });
            this.configuration.Services.Replace(typeof(ICommandHandlerTypeResolver), this.resolver.Object);
            var service = new Mock<ISimpleService>();

            this.container.RegisterInstance(service.Object);
            MessageProcessor processor = this.CreateTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            await processor.ProcessAsync(command);

            // Assert
            service.Verify(s => s.Execute(), Times.Once());
        }
#endif
        
        private MessageProcessor CreateTestableProcessor(ProcessorConfiguration config = null)
        {
            try
            {
                config = config ?? this.configuration;
                config.RegisterContainer(this.container);
                MessageProcessor processor = new MessageProcessor(config);
                this.disposableResources.Add(processor);
                config = null;
                return processor;
            }
            finally
            {
                if (config != null)
                {
                    config.Dispose();
                }
            }
        }

        public class InvalidCommand : ICommand
        {
            [Required]
            public string Property { get; set; }
        }

        public class ValidCommand : ICommand
        {
            public ValidCommand()
            {
                this.Property = "test";
            }

            [Required]
            public string Property { get; set; }
        }

#if LOOSE_CQRS
        public class ValidCommandHandlerWithResult : CommandHandler, ICommandHandler<ValidCommand, string>
        {
            public ValidCommandHandlerWithResult(ISimpleService service)
            {
                this.Service = service;
            }

            public ISimpleService Service { get; set; }

            public string Handle(ValidCommand command)
            {
                this.Service.Execute();
                return "OK";
            }
        }
#endif

        public class ValidCommandHandler : CommandHandler, ICommandHandler<ValidCommand>
        {
            public ValidCommandHandler(ISimpleService service)
            {
                this.Service = service;
            }

            public ISimpleService Service { get; set; }

            public void Handle(ValidCommand command)
            {
                this.Service.Execute();
            }
        }

        public interface ISimpleService
        {
            void Execute();
        }

        public class SimpleService : ISimpleService
        {
            public void Execute()
            {
            }
        }

        public void Dispose()
        {
            this.configuration.Dispose();
            foreach (IDisposable disposable in this.disposableResources)
            {
                disposable.Dispose();
            }
        }
    }
}