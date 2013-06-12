namespace CommandProcessing.Unity.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using CommandProcessing;
    using CommandProcessing.Dispatcher;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public sealed class CommandProcessorWithUnityFixture : IDisposable
    {
        private readonly ICollection<IDisposable> disposableResources = new Collection<IDisposable>();

        private readonly ProcessorConfiguration configuration = new ProcessorConfiguration();
        
        private readonly IUnityContainer container = new UnityContainer();

        private readonly Mock<IHandlerTypeResolver> resolver = new Mock<IHandlerTypeResolver>();

        [TestMethod]
        public void WhenProcessingValidCommandThenCommandIsProcessed()
        {
            // Arrange
            this.resolver
                .Setup(r => r.GetHandlerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new[] { typeof(ValidHandler) });
            this.configuration.Services.Replace(typeof(IHandlerTypeResolver), this.resolver.Object);
            var service = new Mock<ISimpleService>();
            
            this.container.RegisterInstance<ISimpleService>(service.Object);
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<ValidCommand, string>(command);

            // Assert
            Assert.AreEqual("OK", result);
            service.Verify(s => s.Execute(), Times.Once());
        }
        
        [TestMethod]
        public void WhenProcessingCommandWithoutResultThenCommandIsProcessed()
        {
            // Arrange
            this.resolver
                .Setup(r => r.GetHandlerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new[] { typeof(ValidHandlerWithoutResult) });
            this.configuration.Services.Replace(typeof(IHandlerTypeResolver), this.resolver.Object);
            var service = new Mock<ISimpleService>();

            this.container.RegisterInstance<ISimpleService>(service.Object);
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            processor.Process(command);

            // Assert
            service.Verify(s => s.Execute(), Times.Once());
        }
        
        private CommandProcessor CreatTestableProcessor(ProcessorConfiguration config = null)
        {
            try
            {
                config = config ?? this.configuration;
                config.RegisterContainer(this.container);
                CommandProcessor processor = new CommandProcessor(config);
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

        public class InvalidCommand : Command
        {
            [Required]
            public string Property { get; set; }
        }

        public class ValidCommand : Command
        {
            public ValidCommand()
            {
                this.Property = "test";
            }

            [Required]
            public string Property { get; set; }
        }

        public class ValidHandler : Handler<ValidCommand, string>
        {
            public ValidHandler(ISimpleService service)
            {
                this.Service = service;
            }

            public ISimpleService Service { get; set; }

            public override string Handle(ValidCommand command)
            {
                this.Service.Execute();
                return "OK";
            }
        }

        public class ValidHandlerWithoutResult : Handler<ValidCommand>
        {
            public ValidHandlerWithoutResult(ISimpleService service)
            {
                this.Service = service;
            }

            public ISimpleService Service { get; set; }
            
            public override void Handle(ValidCommand command)
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

        [TestCleanup]
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