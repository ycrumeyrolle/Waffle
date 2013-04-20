namespace CommandProcessing.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Caching;
    using System.Threading;
    using CommandProcessing;
    using CommandProcessing.Caching;
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
            var handler = this.SetupHandler<ValidCommand, string>(null, "OK");
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<ValidCommand, string>(command);

            // Assert
            Assert.AreEqual("OK", result);
            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
        }
        
        [TestMethod]
        public void WhenProcessingCommandWithoutResultThenCommandIsProcessed()
        {
            // Arrange
            var handler = this.SetupHandler<ValidCommand>();
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            processor.Process(command);

            // Assert
            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingThrowExceptionWithoutFilterThenThrowsException()
        {
            // Arrange
            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>(null, "ok");
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            ExceptionAssert.Throws<Exception>(() => processor.Process<ValidCommand, string>(command));
            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingThrowExceptionWithUselessFilterThenThrowsException()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> exceptionFilter = new Mock<ExceptionFilterAttribute>(MockBehavior.Strict);
            exceptionFilter.Setup(f => f.OnException(It.IsAny<HandlerExecutedContext>()));
            this.configuration.Filters.Add(exceptionFilter.Object);
            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>(null, "ok");
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            ExceptionAssert.Throws<Exception>(() => processor.Process<ValidCommand, string>(command));
            exceptionFilter.Verify(f => f.OnException(It.IsAny<HandlerExecutedContext>()), Times.Once());
            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
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

            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>(config, "ok");
           
            CommandProcessor processor = this.CreatTestableProcessor(config);
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<ValidCommand, string>(command);

            exceptionFilter.Verify(f => f.OnException(It.IsAny<HandlerExecutedContext>()), Times.Once());
            Assert.AreEqual("Exception !!", result);

            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
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

            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>(null, "ok");
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<ValidCommand, string>(command);

            Assert.AreEqual("Exception !!", result);
            exceptionFilter.Verify(f => f.OnException(It.IsAny<HandlerExecutedContext>()), Times.Once());

            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingWithFiltersThenReturnsValueTransformedByFilters()
        {
            // Arrange
            var filter1 = this.SetupFilter("X");
            var filter2 = this.SetupFilter("Y");
            
            var handler = this.SetupHandler<ValidCommand, string>(null, "OK");
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<ValidCommand, string>(command);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("OK"));
            Assert.IsTrue(result.Contains("X"));
            Assert.IsTrue(result.Contains("Y"));

            filter1.Verify(f => f.OnCommandExecuting(It.IsAny<HandlerContext>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            filter2.Verify(f => f.OnCommandExecuting(It.IsAny<HandlerContext>()), Times.Once());
            filter2.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
        }
        
        [TestMethod]
        public void WhenProcessingWithFilterSettingValueThenReturnsValueFromFilter()
        {
            // Arrange
            var filter1 = this.SetupFilter("X", c => c.Result = "OK from filter");
            
            var handler = this.SetupHandler<ValidCommand, string>(null, "OK");
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<ValidCommand, string>(command);

            Assert.AreEqual("OK from filter", result);
            filter1.Verify(f => f.OnCommandExecuting(It.IsAny<HandlerContext>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Never());

            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Never());
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
                c => c.Result = "Exception !!");
            this.configuration.Filters.Add(exceptionFilter.Object);

            var filter1 = this.SetupFilter("X");
            var filter2 = this.SetupFilter("Y");

            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>(null, "ok");
          
            CommandProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<ValidCommand, string>(command);

            Assert.AreEqual("Exception !!", result);
            filter1.Verify(f => f.OnCommandExecuting(It.IsAny<HandlerContext>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            filter2.Verify(f => f.OnCommandExecuting(It.IsAny<HandlerContext>()), Times.Once());
            filter2.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
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


            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>(null, "ok");
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

            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
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
                .Callback<HandlerExecutedContext>(c =>
                    {
                        if (c.Exception == null)
                        {
                            c.Result += value;
                        }

                        executedCallback(c);
                    });
            this.configuration.Filters.Add(filter.Object);
            return filter;
        }

        private Mock<Handler<TCommand, TResult>> SetupHandlerImpl<TCommand, TResult>(ProcessorConfiguration config = null) where TCommand : ICommand
        {
            config = config ?? this.configuration;
            Mock<Handler<TCommand, TResult>> handler = new Mock<Handler<TCommand, TResult>>();
            Mock<IHandlerActivator> activator = new Mock<IHandlerActivator>(MockBehavior.Strict);
            activator
                .Setup(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()))
                .Returns(handler.Object);
            config.Services.Replace(typeof(IHandlerActivator), activator.Object);

            Mock<IHandlerSelector> selector = new Mock<IHandlerSelector>(MockBehavior.Strict);
            HandlerDescriptor descriptor = new HandlerDescriptor(config, handler.Object.GetType());
            selector
                .Setup(s => s.SelectHandler(It.IsAny<HandlerRequest>()))
                .Returns(descriptor);

            config.Services.Replace(typeof(IHandlerSelector), selector.Object);


            return handler;
        }

        private Mock<Handler<TCommand, object>> SetupThrowingHandler<TCommand, TException>(ProcessorConfiguration config = null) 
            where TCommand : ICommand 
            where TException : Exception, new()
        {
            Mock<Handler<TCommand, object>> handler = this.SetupHandlerImpl<TCommand, object>(config);
            handler
                .Setup(h => h.Handle(It.IsAny<TCommand>()))
                .Throws<TException>();
            return handler;
        }

        private Mock<Handler<TCommand, TResult>> SetupThrowingHandler<TCommand, TResult, TException>(ProcessorConfiguration config = null, TResult result = null)
            where TCommand : ICommand
            where TException : Exception, new()
            where TResult : class
        {
            Mock<Handler<TCommand, TResult>> handler = this.SetupHandlerImpl<TCommand, TResult>(config);
            handler
                .Setup(h => h.Handle(It.IsAny<TCommand>()))
                .Throws<TException>();
            return handler;
        }

        private Mock<Handler<TCommand, TResult>> SetupHandler<TCommand, TResult>(ProcessorConfiguration config = null, TResult result = null)
            where TCommand : ICommand
            where TResult : class
        {
            Mock<Handler<TCommand, TResult>> handler = this.SetupHandlerImpl<TCommand, TResult>(config);
            handler
                .Setup(h => h.Handle(It.IsAny<TCommand>()))
                .Returns(result);
            return handler;
        }

        private Mock<Handler<TCommand, EmptyResult>> SetupHandler<TCommand>(ProcessorConfiguration config = null)
            where TCommand : ICommand
        {
            Mock<Handler<TCommand, EmptyResult>> handler = this.SetupHandlerImpl<TCommand, EmptyResult>(config);
         
            return handler;
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
            HandlerDescriptor descriptor = new HandlerDescriptor(this.configuration, typeof(ValidHandler));
            selector.Setup(s => s.SelectHandler(It.IsAny<HandlerRequest>())).Returns(descriptor);

            this.configuration.Services.Replace(typeof(IHandlerSelector), selector.Object);
            ValidCommand command = new ValidCommand();
            CommandProcessor processor = this.CreatTestableProcessor();

            // Act
            ExceptionAssert.Throws<HandlerNotFoundException>(() => processor.Process<ValidCommand, string>(command));

            // Assert
            activator.Verify(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()), Times.Once());
            selector.Verify(a => a.SelectHandler(It.IsAny<HandlerRequest>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingInvalidCommandThenAbortProcesssing()
        {
            // Arrange
            Mock<IHandlerActivator> activator = new Mock<IHandlerActivator>(MockBehavior.Strict);
            InvalidCommand command = new InvalidCommand();
            activator.Setup(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()));
            this.configuration.Services.Replace(typeof(IHandlerActivator), activator.Object);
            
            var handler = this.SetupHandler<InvalidCommand, string>(null, "OK");

            CommandProcessor processor = this.CreatTestableProcessor();

            // Act
            var result = processor.Process<InvalidCommand, string>(command);

            // Assert
            Assert.IsNull(result);
            activator.Verify(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()), Times.Never());

            handler.Verify(h => h.Handle(It.IsAny<InvalidCommand>()), Times.Never());
        }

        [TestMethod]
        public void WhenProcessingCommandWithCacheThenReturnValueInCache()
        {
            Mock<IHandlerActivator> activator = new Mock<IHandlerActivator>(MockBehavior.Strict);
            ValidCommand command1 = new ValidCommand();
            command1.Property = "test1";
            ValidCommand command2 = new ValidCommand();
            command2.Property = "test2";
            activator.Setup(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()));
            this.configuration.Services.Replace(typeof(IHandlerActivator), activator.Object);

            this.SetupHandler<ValidCommand, string>(null, "OK");

            CommandProcessor processor = this.CreatTestableProcessor();
            
            CacheAttribute attribute = new CacheAttribute();
            attribute.Duration = 10;
            attribute.VaryByParams = "none";
            this.configuration.Filters.Add(attribute);

            // Act
            var result1 = processor.Process<ValidCommand, string>(command1);
            var result2 = processor.Process<ValidCommand, string>(command2);

            // Without result 1 would be different from result2
            // With the cache results are the same
            Assert.AreEqual(result1, result2);
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
                return command.Property;
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