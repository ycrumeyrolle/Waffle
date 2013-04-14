namespace CommandProcessing.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Threading;
    using CommandProcessing;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CommandProcessorFixture : IDisposable
    {
        private readonly ICollection<IDisposable> disposableResources = new Collection<IDisposable>();

        private readonly ProcessorConfiguration configuration = new ProcessorConfiguration();

        private readonly Mock<Handler> handler = new Mock<Handler>();

        private readonly Mock<IDependencyResolver> dependencyResolver = new Mock<IDependencyResolver>(MockBehavior.Loose);
        
        [TestMethod]
        public void WhenCreatingProcessorWithoutConfigThenHasDefaultConfig()
        {
            // Act
            CommandProcessor processor = new CommandProcessor();
            this.disposableResources.Add(processor);

            // Assert
            Assert.IsNotNull(processor.Configuration);
        }

        [TestMethod]
        public void WhenCreatingProcessorWithNullConfigThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => new CommandProcessor(null), "configuration");
        }

        [TestMethod]
        public void WhenCreatingProcessorWithCustomConfigThenHasCustomConfig()
        {
            // Arrange
            ProcessorConfiguration config = new ProcessorConfiguration();

            // Act
            CommandProcessor processor = this.CreatTestableProcessor(config);
            this.disposableResources.Add(processor);

            // Assert
            Assert.IsNotNull(processor.Configuration);
            Assert.AreSame(config, processor.Configuration);
        }

        [TestMethod]
        public void WhenProcessingValidCommandThenCommandIsProcessed()
        {
            // Arrange
            this.SetupValidHandler<ValidCommand>();
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<ValidCommand, string>(command);

            // Assert
            Assert.AreEqual("OK", result);
            this.handler.Verify(h => h.Handle(It.IsAny<ICommand>()), Times.Once());
        }
        
        [TestMethod]
        public void WhenProcessingCommandWithoutResultThenCommandIsProcessed()
        {
            // Arrange
            this.SetupValidHandler<ValidCommand>();
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            this.handler
                .Setup(h => h.Handle(It.IsAny<ICommand>()))
                .Returns(null);

            // Act
            processor.Process(command);

            // Assert
            this.handler.Verify(h => h.Handle(It.IsAny<ICommand>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingThrowExceptionWithoutFilterThenThrowsException()
        {
            // Arrange
            this.SetupValidHandler<ValidCommand>();
            this.handler.Setup(h => h.Handle(It.IsAny<ValidCommand>())).Throws<Exception>();
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            ExceptionAssert.Throws<Exception>(() => processor.Process<ValidCommand, string>(command));
            this.handler.Verify(h => h.Handle(It.IsAny<ICommand>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingThrowExceptionWithUselessFilterThenThrowsException()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> exceptionFilter = new Mock<ExceptionFilterAttribute>(MockBehavior.Strict);
            exceptionFilter.Setup(f => f.OnException(It.IsAny<HandlerExecutedContext>()));
            this.configuration.Filters.Add(exceptionFilter.Object);
            this.SetupValidHandler<ValidCommand>();
            this.handler
                .Setup(h => h.Handle(It.IsAny<ValidCommand>()))
                .Throws<Exception>();
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            ExceptionAssert.Throws<Exception>(() => processor.Process<ValidCommand, string>(command));
            exceptionFilter.Verify(f => f.OnException(It.IsAny<HandlerExecutedContext>()), Times.Once());
            this.handler.Verify(h => h.Handle(It.IsAny<ICommand>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingThrowExceptionWithHandlingFilterThenReturnsExceptionValue()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> exceptionFilter = new Mock<ExceptionFilterAttribute>(MockBehavior.Strict);
            exceptionFilter
                .Setup(f => f.OnException(It.IsAny<HandlerExecutedContext>()))
                .Callback<HandlerExecutedContext>(c => c.Result = "Exception !!");
            ProcessorConfiguration config = new ProcessorConfiguration();
            config.Filters.Add(exceptionFilter.Object);

            this.SetupValidHandler<ValidCommand>(config);
            this.handler
                .Setup(h => h.Handle(It.IsAny<ValidCommand>()))
                .Throws<Exception>();
            CommandProcessor processor = this.CreatTestableProcessor(config);
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<ValidCommand, string>(command);

            exceptionFilter.Verify(f => f.OnException(It.IsAny<HandlerExecutedContext>()), Times.Once());
            Assert.AreEqual("Exception !!", result);

            this.handler.Verify(h => h.Handle(It.IsAny<ICommand>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingThrowExceptionWithHandlingFilterAndResultThenReturnsExceptionValue()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> exceptionFilter = new Mock<ExceptionFilterAttribute>(MockBehavior.Strict);
            exceptionFilter
                .Setup(f => f.OnException(It.IsAny<HandlerExecutedContext>()))
                .Callback<HandlerExecutedContext>(c => c.Result = "Exception !!");
            this.configuration.Filters.Add(exceptionFilter.Object);
            this.SetupValidHandler<ValidCommand>();
            this.handler
                .Setup(h => h.Handle(It.IsAny<ValidCommand>()))
                .Throws<Exception>();
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<ValidCommand, string>(command);

            Assert.AreEqual("Exception !!", result);
            exceptionFilter.Verify(f => f.OnException(It.IsAny<HandlerExecutedContext>()), Times.Once());

            this.handler.Verify(h => h.Handle(It.IsAny<ICommand>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingWithFiltersThenReturnsValueTransformedByFilters()
        {
            // Arrange
            var filter1 = this.SetupFilter("X");
            var filter2 = this.SetupFilter("Y");

            this.SetupValidHandler<ValidCommand>();
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<ValidCommand, string>(command);

            Assert.AreEqual("OK" + "X" + "Y", result);
            filter1.Verify(f => f.OnCommandExecuting(It.IsAny<HandlerContext>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            filter2.Verify(f => f.OnCommandExecuting(It.IsAny<HandlerContext>()), Times.Once());
            filter2.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            this.handler.Verify(h => h.Handle(It.IsAny<ICommand>()), Times.Once());
        }
        
        [TestMethod]
        public void WhenProcessingWithFilterSettingValueThenReturnsValueFromFilter()
        {
            // Arrange
            var filter1 = this.SetupFilter("X", c => c.Result = "OK from filter");

            this.SetupValidHandler<ValidCommand>();
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<ValidCommand, string>(command);

            Assert.AreEqual("OK from filter", result);
            filter1.Verify(f => f.OnCommandExecuting(It.IsAny<HandlerContext>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Never());

            this.handler.Verify(h => h.Handle(It.IsAny<ICommand>()), Times.Never());
        }

        [TestMethod]
        public void WhenProcessingWithFiltersThrowExceptionThenHandlesExceptionByExceptionFilter()
        {
            // Arrange
            this.configuration.Filters.Clear(); 
            Mock<ExceptionFilterAttribute> exceptionFilter = new Mock<ExceptionFilterAttribute>(MockBehavior.Strict);
            exceptionFilter
                .Setup(f => f.OnException(It.IsAny<HandlerExecutedContext>()))
                .Callback<HandlerExecutedContext>(
                c => {
                    c.Result = "Exception !!"; 
                });
            this.configuration.Filters.Add(exceptionFilter.Object);

            var filter1 = this.SetupFilter("X");
            var filter2 = this.SetupFilter("Y");

            this.SetupValidHandler<ValidCommand>();
            this.handler
                .Setup(h => h.Handle(It.IsAny<ValidCommand>()))
                .Throws<Exception>();
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<ValidCommand, string>(command);

            Assert.AreEqual("Exception !!", result);
            filter1.Verify(f => f.OnCommandExecuting(It.IsAny<HandlerContext>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            filter2.Verify(f => f.OnCommandExecuting(It.IsAny<HandlerContext>()), Times.Once());
            filter2.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            this.handler.Verify(h => h.Handle(It.IsAny<ICommand>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingWithFiltersThrowExceptionThenHandlesExceptionByHandlerFilter()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> exceptionFilter = new Mock<ExceptionFilterAttribute>(MockBehavior.Strict);
            exceptionFilter
                .Setup(f => f.OnException(It.IsAny<HandlerExecutedContext>()))
                .Callback<HandlerExecutedContext>(c => c.Result = "Exception !!");
            this.configuration.Filters.Add(exceptionFilter.Object);
            
            var filter1 = this.SetupFilter("X");
            var filter2 = this.SetupFilter("Y");
            var filter3 = this.SetupFilter("Z");

            this.SetupValidHandler<ValidCommand>();
            this.handler
                .Setup(h => h.Handle(It.IsAny<ValidCommand>()))
                .Throws<Exception>();
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<ValidCommand, string>(command);

            // Assert
            // Exception filter overrides result of handler filters
            Assert.AreEqual("Exception !!", result);
            filter1.Verify(f => f.OnCommandExecuting(It.IsAny<HandlerContext>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            filter2.Verify(f => f.OnCommandExecuting(It.IsAny<HandlerContext>()), Times.Once());
            filter2.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            filter3.Verify(f => f.OnCommandExecuting(It.IsAny<HandlerContext>()), Times.Once());
            filter3.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            this.handler.Verify(h => h.Handle(It.IsAny<ICommand>()), Times.Once());
        }

        private Mock<HandlerFilterAttribute> SetupFilter(string value, Action<HandlerContext> executingCallback = null, Action<HandlerExecutedContext> executedCallback = null)
        {
            executingCallback = executingCallback ?? new Action<HandlerContext>(_ => { });
            executedCallback = executedCallback ?? new Action<HandlerExecutedContext>(_ => { });
            Mock<HandlerFilterAttribute> filter = new Mock<HandlerFilterAttribute>(MockBehavior.Strict);
            filter.Setup(f => f.AllowMultiple).Returns(true);
            filter
                .Setup(f => f.OnCommandExecuting(It.IsAny<HandlerContext>()))
                .Callback<HandlerContext>(c => executingCallback(c));
            filter
                .Setup(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()))
                .Callback<HandlerExecutedContext>(c => {
                    if(c.Exception == null) 
                        c.Result += value; 
                    executedCallback(c); 
                });
            this.configuration.Filters.Add(filter.Object);
            return filter;
        }

        private void SetupValidHandler<TCommand>(ProcessorConfiguration config = null) where TCommand : ICommand
        {
            config = config ?? this.configuration;
            Mock<IHandlerActivator> activator = new Mock<IHandlerActivator>(MockBehavior.Strict);
            activator
                .Setup(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()))
                .Returns(this.handler.Object);
            config.Services.Replace(typeof(IHandlerActivator), activator.Object);

            Mock<IHandlerSelector> selector = new Mock<IHandlerSelector>(MockBehavior.Strict);
            HandlerDescriptor descriptor = new HandlerDescriptor(config, this.handler.Object.GetType());
            selector
                .Setup(s => s.SelectHandler(It.IsAny<HandlerRequest>()))
                .Returns(descriptor);

            config.Services.Replace(typeof(IHandlerSelector), selector.Object);

            this.handler
                .Setup(h => h.Handle(It.IsAny<TCommand>()))
                .Returns("OK");
        }

        [TestMethod]
        public void WhenProcessingUnknowHandlerThenThrowsHandlerNotFoundException()
        {
            // Arrange
            Mock<IHandlerActivator> activator = new Mock<IHandlerActivator>(MockBehavior.Strict);
            activator
                .Setup(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()))
                .Returns<Handler>(null);
            this.configuration.Services.Replace(typeof(IHandlerActivator), activator.Object);

            Mock<IHandlerSelector> selector = new Mock<IHandlerSelector>(MockBehavior.Strict);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.configuration, this.handler.Object.GetType());
            selector.Setup(s => s.SelectHandler(It.IsAny<HandlerRequest>())).Returns(descriptor);

            this.configuration.Services.Replace(typeof(IHandlerSelector), selector.Object);
            ValidCommand command = new ValidCommand();
            CommandProcessor processor = this.CreatTestableProcessor();

            // Act
            ExceptionAssert.Throws<HandlerNotFoundException>(() => processor.Process<ValidCommand, string>(command));

            // Assert
            activator.Verify(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()), Times.Once());
            selector.Verify(a => a.SelectHandler(It.IsAny<HandlerRequest>()), Times.Once());

            this.handler.Verify(h => h.Handle(It.IsAny<ICommand>()), Times.Never());
        }

        [TestMethod]
        public void WhenProcessingInvalidCommandThenAbortProcesssing()
        {
            // Arrange
            Mock<IHandlerActivator> activator = new Mock<IHandlerActivator>(MockBehavior.Strict);
            InvalidCommand command = new InvalidCommand();
            activator.Setup(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()));
            this.configuration.Services.Replace(typeof(IHandlerActivator), activator.Object);

            this.SetupValidHandler<InvalidCommand>();

            CommandProcessor processor = this.CreatTestableProcessor();

            // Act
            var result = processor.Process<InvalidCommand, string>(command);

            // Assert
            Assert.IsNull(result);
            activator.Verify(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()), Times.Never());

            this.handler.Verify(h => h.Handle(It.IsAny<ICommand>()), Times.Never());
        }

        [TestMethod]
        public void WhenUsingUnknowServiceThenThrowsArgumentException()
        {
            // Arrange
            CommandProcessor processor = this.CreatTestableProcessor();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => processor.Using<ISimpleService>());
        }

        [TestMethod]
        public void WhenUsingServiceWithProxyThenReturnsServiceProxy()
        {
            // Arrange
            SimpleService service = new SimpleService();
            this.dependencyResolver.Setup(d => d.GetService(typeof(ISimpleService))).Returns(service);
            this.configuration.ServiceProxyCreationEnabled = true;

            CommandProcessor processor = this.CreatTestableProcessor();

            // Act
            var serviceProxy = processor.Using<ISimpleService>();

            // Assert
            Assert.IsNotNull(serviceProxy);
            Assert.IsInstanceOfType(serviceProxy, service.GetType());
            Assert.IsFalse(serviceProxy.GetType() == service.GetType());
        }

        [TestMethod]
        public void WhenUsingServiceWithoutProxyThenReturnsService()
        {
            // Arrange
            SimpleService service = new SimpleService();
            this.dependencyResolver.Setup(d => d.GetService(typeof(ISimpleService))).Returns(service); 
            this.configuration.ServiceProxyCreationEnabled = false;

            CommandProcessor processor = this.CreatTestableProcessor();

            // Act
            var serviceDirect = processor.Using<ISimpleService>();

            // Assert
            Assert.IsNotNull(serviceDirect);
            Assert.IsInstanceOfType(serviceDirect, service.GetType());
            Assert.IsTrue(serviceDirect.GetType() == service.GetType());
        }

        private CommandProcessor CreatTestableProcessor(ProcessorConfiguration config = null)
        {
            try
            {
                config = config ?? this.configuration;
                this.dependencyResolver.Setup(r => r.BeginScope()).Returns(this.dependencyResolver.Object);
                config.DependencyResolver = this.dependencyResolver.Object;
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
            public override string Handle(ValidCommand command)
            {
                return "OK";
            }
        }

        public class ValidHandlerWithoutResult : Handler<ValidCommand>
        {
            public override void Handle(ValidCommand command)
            {
                return;
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
            foreach (IDisposable disposable in this.disposableResources)
            {
                disposable.Dispose();
            }
        }
    }
}