namespace Waffle.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle;
    using Waffle.Caching;
    using Waffle.Commands;
    using Waffle.Dependencies;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;

    [TestClass]
    public sealed class CommandProcessorFixture : IDisposable
    {
        private readonly ICollection<IDisposable> disposableResources = new Collection<IDisposable>();

        private readonly ProcessorConfiguration configuration = new ProcessorConfiguration();

        private readonly Mock<IDependencyResolver> dependencyResolver = new Mock<IDependencyResolver>(MockBehavior.Loose);

        [TestMethod]
        public void WhenCreatingProcessorWithoutConfigThenHasDefaultConfig()
        {
            // Act
            MessageProcessor processor = new MessageProcessor();
            this.disposableResources.Add(processor);

            // Assert
            Assert.IsNotNull(processor.Configuration);
        }

        [TestMethod]
        public void WhenCreatingProcessorWithNullConfigThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => new MessageProcessor(null), "configuration");
        }

        [TestMethod]
        public void WhenCreatingProcessorWithCustomConfigThenHasCustomConfig()
        {
            // Arrange
            ProcessorConfiguration config = new ProcessorConfiguration();

            // Act
            MessageProcessor processor = this.CreatTestableProcessor(config);
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
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();
            
            // Act
            var result = processor.Process<string>(command);

            // Assert
            Assert.AreEqual("OK", result);
            handler.Verify(h => h.Handle(It.IsAny<ICommand>(), It.IsAny<CommandHandlerContext>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingCommandWithoutResultThenCommandIsProcessed()
        {
            // Arrange
            var handler = this.SetupHandler<ValidCommand>();
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            processor.Process(command);

            // Assert
            handler.Verify(h => h.Handle(It.IsAny<ICommand>(), It.IsAny<CommandHandlerContext>()), Times.Once());
        }
        
        [TestMethod]
        public void WhenProcessingMultipleCommandThenCommandsAreProcessed()
        {
            // Arrange
            Mock<ISpy> spy = new Mock<ISpy>(MockBehavior.Strict);
            spy.Setup(s => s.Spy("MultipleCommand1"));
            spy.Setup(s => s.Spy("MultipleCommand2"));
            MultipleCommandCommandHandler commandCommandHandler = new MultipleCommandCommandHandler(spy.Object);
            this.SetupHandler<MultipleCommand1, string>(commandCommandHandler);
            MessageProcessor processor = this.CreatTestableProcessor();
            MultipleCommand1 command1 = new MultipleCommand1();
            MultipleCommand2 command2 = new MultipleCommand2();
            MultipleCommand2 command3 = new MultipleCommand2();

            // Act & assert
            processor.Process<string>(command1);
            spy.Verify(h => h.Spy("MultipleCommand1"), Times.Once());
            spy.Verify(h => h.Spy("MultipleCommand2"), Times.Never());
            
            // Handler must be configured now to avoid incorrect descriptor creation
            this.SetupHandler<MultipleCommand2, string>(commandCommandHandler);
            processor.Process<string>(command2);

            spy.Verify(h => h.Spy("MultipleCommand1"), Times.Once());
            spy.Verify(h => h.Spy("MultipleCommand2"), Times.Once());

            processor.Process<string>(command2);
            spy.Verify(h => h.Spy("MultipleCommand1"), Times.Once());
            spy.Verify(h => h.Spy("MultipleCommand2"), Times.Exactly(2));
        }
        
        [TestMethod]
        public void WhenProcessingThrowExceptionWithoutFilterThenThrowsException()
        {
            // Arrange
            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>();
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            ExceptionAssert.Throws<Exception>(() => processor.Process<string>(command));
            handler.Verify(h => h.Handle(It.IsAny<ICommand>(), It.IsAny<CommandHandlerContext>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingThrowExceptionWithUselessFilterThenThrowsException()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> exceptionFilter = new Mock<ExceptionFilterAttribute>(MockBehavior.Strict);
            exceptionFilter.Setup(f => f.OnException(It.IsAny<HandlerExecutedContext>()));
            this.configuration.Filters.Add(exceptionFilter.Object);
            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>();
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            ExceptionAssert.Throws<Exception>(() => processor.Process<string>(command));
            exceptionFilter.Verify(f => f.OnException(It.IsAny<HandlerExecutedContext>()), Times.Once());
            handler.Verify(h => h.Handle(It.IsAny<ICommand>(), It.IsAny<CommandHandlerContext>()), Times.Once());
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

            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>(config);

            MessageProcessor processor = this.CreatTestableProcessor(config);
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<string>(command);

            exceptionFilter.Verify(f => f.OnException(It.IsAny<HandlerExecutedContext>()), Times.Once());
            Assert.AreEqual("Exception !!", result);

            handler.Verify(h => h.Handle(It.IsAny<ICommand>(), It.IsAny<CommandHandlerContext>()), Times.Once());
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

            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>();
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<string>(command);

            Assert.AreEqual("Exception !!", result);
            exceptionFilter.Verify(f => f.OnException(It.IsAny<HandlerExecutedContext>()), Times.Once());

            handler.Verify(h => h.Handle(It.IsAny<ICommand>(), It.IsAny<CommandHandlerContext>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingWithFiltersThenReturnsValueTransformedByFilters()
        {
            // Arrange
            var filter1 = this.SetupFilter("X");
            var filter2 = this.SetupFilter("Y");

            var handler = this.SetupHandler<ValidCommand, string>(null, "OK");
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<string>(command);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("OK"));
            Assert.IsTrue(result.Contains("X"));
            Assert.IsTrue(result.Contains("Y"));

            filter1.Verify(f => f.OnCommandExecuting(It.IsAny<CommandHandlerContext>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            filter2.Verify(f => f.OnCommandExecuting(It.IsAny<CommandHandlerContext>()), Times.Once());
            filter2.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            handler.Verify(h => h.Handle(It.IsAny<ICommand>(), It.IsAny<CommandHandlerContext>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingWithFilterSettingValueThenReturnsValueFromFilter()
        {
            // Arrange
            var filter1 = this.SetupFilter("X", c => c.Result = "OK from filter");

            var handler = this.SetupHandler<ValidCommand, string>(null, "OK");
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<string>(command);

            Assert.AreEqual("OK from filter", result);
            filter1.Verify(f => f.OnCommandExecuting(It.IsAny<CommandHandlerContext>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Never());

            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>(), It.IsAny<CommandHandlerContext>()), Times.Never());
        }

        [TestMethod]
        public void WhenProcessingWithFiltersThrowExceptionThenHandlesExceptionByExceptionFilter()
        {
            // Arrange
            this.configuration.Filters.Clear();
            Mock<ExceptionFilterAttribute> exceptionFilter = new Mock<ExceptionFilterAttribute>(MockBehavior.Strict);
            exceptionFilter
                .Setup(f => f.OnException(It.IsAny<HandlerExecutedContext>()))
                .Callback<HandlerExecutedContext>(c => c.Result = "Exception !!");
            this.configuration.Filters.Add(exceptionFilter.Object);

            var filter1 = this.SetupFilter("X");
            var filter2 = this.SetupFilter("Y");

            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>();

            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<string>(command);

            Assert.AreEqual("Exception !!", result);
            filter1.Verify(f => f.OnCommandExecuting(It.IsAny<CommandHandlerContext>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            filter2.Verify(f => f.OnCommandExecuting(It.IsAny<CommandHandlerContext>()), Times.Once());
            filter2.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            handler.Verify(h => h.Handle(It.IsAny<ICommand>(), It.IsAny<CommandHandlerContext>()), Times.Once());
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


            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>();
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = processor.Process<string>(command);

            // Assert
            // Exception filter overrides result of handler filters
            Assert.AreEqual("Exception !!", result);
            filter1.Verify(f => f.OnCommandExecuting(It.IsAny<CommandHandlerContext>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            filter2.Verify(f => f.OnCommandExecuting(It.IsAny<CommandHandlerContext>()), Times.Once());
            filter2.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            filter3.Verify(f => f.OnCommandExecuting(It.IsAny<CommandHandlerContext>()), Times.Once());
            filter3.Verify(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()), Times.Once());

            handler.Verify(h => h.Handle(It.IsAny<ICommand>(), It.IsAny<CommandHandlerContext>()), Times.Once());
        }

        private Mock<CommandHandlerFilterAttribute> SetupFilter(string value, Action<CommandHandlerContext> executingCallback = null, Action<HandlerExecutedContext> executedCallback = null)
        {
            executingCallback = executingCallback ?? (_ => { });
            executedCallback = executedCallback ?? (_ => { });
            Mock<CommandHandlerFilterAttribute> filter = new Mock<CommandHandlerFilterAttribute>(MockBehavior.Strict);
            filter.Setup(f => f.AllowMultiple).Returns(true);
            filter
                .Setup(f => f.OnCommandExecuting(It.IsAny<CommandHandlerContext>()))
                .Callback<CommandHandlerContext>(c => executingCallback(c));
            filter
                .Setup(f => f.OnCommandExecuted(It.IsAny<HandlerExecutedContext>()))
                .Callback<HandlerExecutedContext>(c =>
                {
                    if (c.Exception == null)
                    {
                        c.Result +=  value;
                    }

                    executedCallback(c);
                });
            this.configuration.Filters.Add(filter.Object);
            return filter;
        }

        private Mock<ICommandHandler<TCommand, TResult>> SetupHandlerImpl<TCommand, TResult>(ProcessorConfiguration config = null) where TCommand : ICommand
        {
            config = config ?? this.configuration;
            Mock<ICommandHandler<TCommand, TResult>> handler = new Mock<ICommandHandler<TCommand, TResult>>();
            Mock<ICommandHandlerActivator> activator = new Mock<ICommandHandlerActivator>(MockBehavior.Strict);
            activator
                .Setup(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()))
                .Returns(handler.Object);
            config.Services.Replace(typeof(ICommandHandlerActivator), activator.Object);

            Mock<ICommandHandlerSelector> selector = new Mock<ICommandHandlerSelector>(MockBehavior.Strict);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(config, typeof(TCommand), handler.Object.GetType());
            selector
                .Setup(s => s.SelectHandler(It.IsAny<CommandHandlerRequest>()))
                .Returns(descriptor);

            config.Services.Replace(typeof(ICommandHandlerSelector), selector.Object);


            return handler;
        }

        private CommandHandler SetupHandlerImpl<TCommand, TResult>(CommandHandler commandHandler) where TCommand : ICommand
        {
            ProcessorConfiguration config = this.configuration;
            Mock<ICommandHandlerActivator> activator = new Mock<ICommandHandlerActivator>(MockBehavior.Strict);
            activator
                .Setup(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()))
                .Returns(commandHandler);
            config.Services.Replace(typeof(ICommandHandlerActivator), activator.Object);

            Mock<ICommandHandlerSelector> selector = new Mock<ICommandHandlerSelector>(MockBehavior.Strict);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(config, typeof(TCommand), commandHandler.GetType());
            selector
                .Setup(s => s.SelectHandler(It.IsAny<CommandHandlerRequest>()))
                .Returns(descriptor);

            config.Services.Replace(typeof(ICommandHandlerSelector), selector.Object);
            
            return commandHandler;
        }

        private Mock<ICommandHandler<TCommand, object>> SetupThrowingHandler<TCommand, TException>(ProcessorConfiguration config = null)
            where TCommand : ICommand
            where TException : Exception, new()
        {
            Mock<ICommandHandler<TCommand, object>> handler = this.SetupHandlerImpl<TCommand, object>(config);
            handler
                .Setup(h => h.Handle(It.IsAny<ICommand>(), It.IsAny<CommandHandlerContext>()))
                .Throws<TException>();
            return handler;
        }

        private Mock<ICommandHandler<TCommand, TResult>> SetupThrowingHandler<TCommand, TResult, TException>(ProcessorConfiguration config = null)
            where TCommand : ICommand
            where TException : Exception, new()
            where TResult : class
        {
            Mock<ICommandHandler<TCommand, TResult>> handler = this.SetupHandlerImpl<TCommand, TResult>(config);
            handler
                .Setup(h => h.Handle(It.IsAny<ICommand>(), It.IsAny<CommandHandlerContext>()))
                .Throws<TException>();
            return handler;
        }

        private Mock<ICommandHandler<TCommand, TResult>> SetupHandler<TCommand, TResult>(ProcessorConfiguration config = null, TResult result = null)
            where TCommand : ICommand
            where TResult : class
        {
            Mock<ICommandHandler<TCommand, TResult>> handler = this.SetupHandlerImpl<TCommand, TResult>(config);
            handler
                .Setup(h => h.Handle(It.IsAny<ICommand>(), It.IsAny<CommandHandlerContext>()))
                .Returns(result);
            return handler;
        }

        private Mock<ICommandHandler<TCommand, TResult>> SetupHandler<TCommand, TResult>(ProcessorConfiguration config = null, Func<TResult> funcResult = null)
            where TCommand : ICommand
            where TResult : class
        {
            Mock<ICommandHandler<TCommand, TResult>> handler = this.SetupHandlerImpl<TCommand, TResult>(config);
            handler
                .Setup(h => h.Handle(It.IsAny<ICommand>(), It.IsAny<CommandHandlerContext>()))
                .Returns(funcResult);
            return handler;
        }

        private Mock<ICommandHandler<TCommand, VoidResult>> SetupHandler<TCommand>(ProcessorConfiguration config = null)
            where TCommand : ICommand
        {
            Mock<ICommandHandler<TCommand, VoidResult>> handler = this.SetupHandlerImpl<TCommand, VoidResult>(config);
            handler.Setup(h => h.Handle(It.IsAny<ICommand>(), It.IsAny<CommandHandlerContext>()));
            return handler;
        }
        
        private ICommandHandler<TCommand, TResult> SetupHandler<TCommand, TResult>(CommandHandler commandHandler)
            where TCommand : ICommand
        {
            return (ICommandHandler<TCommand, TResult>)this.SetupHandlerImpl<TCommand, TResult>(commandHandler);
        }

        [TestMethod]
        public void WhenProcessingUnknowHandlerThenThrowsHandlerNotFoundException()
        {
            // Arrange
            Mock<ICommandHandlerActivator> activator = new Mock<ICommandHandlerActivator>(MockBehavior.Strict);
            activator
                .Setup(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()))
                .Returns<CommandHandler>(null);
            this.configuration.Services.Replace(typeof(ICommandHandlerActivator), activator.Object);

            Mock<ICommandHandlerSelector> selector = new Mock<ICommandHandlerSelector>(MockBehavior.Strict);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.configuration, typeof(ValidCommand), typeof(ValidCommandHandler));
            selector.Setup(s => s.SelectHandler(It.IsAny<CommandHandlerRequest>())).Returns(descriptor);

            this.configuration.Services.Replace(typeof(ICommandHandlerSelector), selector.Object);
            ValidCommand command = new ValidCommand();
            MessageProcessor processor = this.CreatTestableProcessor();

            // Act
            ExceptionAssert.Throws<CommandHandlerNotFoundException>(() => processor.ProcessAsync<string>(command));

            // Assert
            activator.Verify(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()), Times.Once());
            selector.Verify(a => a.SelectHandler(It.IsAny<CommandHandlerRequest>()), Times.Once());
        }

        [TestMethod]
        public void WhenProcessingInvalidCommandThenAbortProcesssing()
        {
            // Arrange
            Mock<ICommandHandlerActivator> activator = new Mock<ICommandHandlerActivator>(MockBehavior.Strict);
            InvalidCommand command = new InvalidCommand();
            activator.Setup(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()));
            this.configuration.Services.Replace(typeof(ICommandHandlerActivator), activator.Object);

            var handler = this.SetupHandler<InvalidCommand, string>(null, "OK");

            MessageProcessor processor = this.CreatTestableProcessor();

            // Act
            var result = processor.Process<string>(command);

            // Assert
            Assert.IsNull(result);
            activator.Verify(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()), Times.Never());

            handler.Verify(h => h.Handle(It.IsAny<InvalidCommand>(), It.IsAny<CommandHandlerContext>()), Times.Never());
        }

        [TestMethod]
        public void WhenProcessingDifferentsCommandsWithCacheVaryByAllThenReturnNewValue()
        {
            Mock<ICommandHandlerActivator> activator = new Mock<ICommandHandlerActivator>(MockBehavior.Strict);
            ValidCommand command1 = new ValidCommand();
            command1.Property = "test1";
            ValidCommand command2 = new ValidCommand();
            command2.Property = "test2";
            activator.Setup(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()));
            this.configuration.Services.Replace(typeof(ICommandHandlerActivator), activator.Object);
            int i = 0;
            this.SetupHandler<ValidCommand, string>(null, () => (i++).ToString(CultureInfo.InvariantCulture));

            MessageProcessor processor = this.CreatTestableProcessor();

            CacheAttribute attribute = new CacheAttribute();
            attribute.Duration = 10;
            attribute.VaryByParams = CacheAttribute.VaryByParamsAll;
            this.configuration.Filters.Add(attribute);

            // Act
            var result1 = processor.ProcessAsync<string>(command1);
            var result2 = processor.ProcessAsync<string>(command2);

            Assert.AreNotEqual(result1, result2);
        }

        [TestMethod]
        public void WhenProcessingDifferentsCommandsWithCacheVaryByNoneThenReturnValueInCache()
        {
            Mock<ICommandHandlerActivator> activator = new Mock<ICommandHandlerActivator>(MockBehavior.Strict);
            ValidCommand command1 = new ValidCommand();
            command1.Property = "test1";
            ValidCommand command2 = new ValidCommand();
            command2.Property = "test2";
            activator.Setup(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()));
            this.configuration.Services.Replace(typeof(ICommandHandlerActivator), activator.Object);
            int i = 0;
            this.SetupHandler<ValidCommand, string>(null, () => (i++).ToString(CultureInfo.InvariantCulture));

            MessageProcessor processor = this.CreatTestableProcessor();

            CacheAttribute attribute = new CacheAttribute();
            attribute.Duration = 10;
            attribute.VaryByParams = CacheAttribute.VaryByParamsNone;
            this.configuration.Filters.Add(attribute);

            // Act
            var result1 = processor.Process<string>(command1);
            var result2 = processor.Process<string>(command2);

            // Without cache result1 would be different from result2
            // With the cache results are the same
            Assert.AreEqual(result1, result2);
        }

        [TestMethod]
        public void WhenProcessingCyclicCommandWithCacheThenReturnValueInCache()
        {
            Mock<ICommandHandlerActivator> activator = new Mock<ICommandHandlerActivator>(MockBehavior.Strict);
            ComplexCyclicCommand command1 = new ComplexCyclicCommand();
            ComplexCyclicCommand command2 = new ComplexCyclicCommand();
            activator.Setup(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()));
            this.configuration.Services.Replace(typeof(ICommandHandlerActivator), activator.Object);

            int i = 0;
            this.SetupHandler<ComplexCyclicCommand, string>(null, () => (i++).ToString(CultureInfo.InvariantCulture));

            MessageProcessor processor = this.CreatTestableProcessor();

            CacheAttribute attribute = new CacheAttribute();
            attribute.Duration = 10;
            attribute.VaryByParams = CacheAttribute.VaryByParamsAll;
            this.configuration.Filters.Add(attribute);

            // Act
            var result1 = processor.Process<string>(command1);
            var result2 = processor.Process<string>(command2);

            // Without cache result 1 would be different from result2
            // With the cache results are the same
            Assert.AreEqual(result1, result2);
        }
        
        [TestMethod]
        public void WhenProcessingDifferentCyclicCommandsWithCacheThenReturnValueInCacheBecauseItIsNotManaged()
        {
            Mock<ICommandHandlerActivator> activator = new Mock<ICommandHandlerActivator>(MockBehavior.Strict);
            ComplexCyclicCommand command1 = new ComplexCyclicCommand();
            ComplexCyclicCommand command2 = new ComplexCyclicCommand();
            command2.Property2 = command1.Property2;
            activator.Setup(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()));
            this.configuration.Services.Replace(typeof(ICommandHandlerActivator), activator.Object);

            int i = 0;
            this.SetupHandler<ComplexCyclicCommand, string>(null, () => (i++).ToString(CultureInfo.InvariantCulture));

            MessageProcessor processor = this.CreatTestableProcessor();

            CacheAttribute attribute = new CacheAttribute();
            attribute.Duration = 10;
            attribute.VaryByParams = CacheAttribute.VaryByParamsAll;
            this.configuration.Filters.Add(attribute);

            // Act
            var result1 = processor.Process<string>(command1);
            var result2 = processor.Process<string>(command2);

            // Without cache result 1 would be different from result2
            // With the cache results are the same
            Assert.AreEqual(result1, result2);
        }

        [TestMethod]
        public void WhenUsingUnknowServiceThenThrowsArgumentException()
        {
            // Arrange
            MessageProcessor processor = this.CreatTestableProcessor();

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

            MessageProcessor processor = this.CreatTestableProcessor();

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

            MessageProcessor processor = this.CreatTestableProcessor();

            // Act
            var serviceDirect = processor.Using<ISimpleService>();

            // Assert
            Assert.IsNotNull(serviceDirect);
            Assert.IsInstanceOfType(serviceDirect, service.GetType());
            Assert.IsTrue(serviceDirect.GetType() == service.GetType());
        }

        private MessageProcessor CreatTestableProcessor(ProcessorConfiguration config = null)
        {
            try
            {
                config = config ?? this.configuration;
             //   config.EnableDefaultTracing();
                this.dependencyResolver.Setup(r => r.BeginScope()).Returns(this.dependencyResolver.Object);
                config.DependencyResolver = this.dependencyResolver.Object;
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

        public class ValidCommandHandler : CommandHandler<ValidCommand, string>
        {
            public override string Handle(ValidCommand command, CommandHandlerContext context)
            {
                return command.Property;
            }
        }

        public class ValidCommandHandlerWithoutResult : CommandHandler<ValidCommand>
        {
            public override void Handle(ValidCommand command, CommandHandlerContext context)
            {
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

        public class CyclicCommand : Command
        {
            public CyclicCommand()
            {
                this.Property1 = "test";
                this.Property2 = this;
            }

            [Required]
            public string Property1 { get; set; }

            [Required]
            public CyclicCommand Property2 { get; set; }
        }

        public class ComplexCyclicCommand : Command
        {
            public ComplexCyclicCommand()
            {
                this.Property1 = "test";
                this.Property2 = new InnerCommand(this);
            }

            [Required]
            public string Property1 { get; set; }

            [Required]
            public InnerCommand Property2 { get; set; }

            public class InnerCommand
            {
                public InnerCommand()
                {
                    this.Property1 = "test";
                }

                public InnerCommand(ComplexCyclicCommand complexCyclicCommand)
                {
                    this.Property1 = "test";
                    this.Property2 = new InnerInnerCommand(complexCyclicCommand);
                }

                [Required]
                public string Property1 { get; set; }

                [Required]
                public InnerInnerCommand Property2 { get; set; }

                public class InnerInnerCommand
                {
                    public InnerInnerCommand(ComplexCyclicCommand complexCyclicCommand)
                    {
                        this.Property1 = "test";
                        this.Property2 = complexCyclicCommand;
                    }

                    [Required]
                    public string Property1 { get; set; }

                    [Required]
                    public ComplexCyclicCommand Property2 { get; set; }
                }
            }
        }


        public interface ISpy
        {
            void Spy(string category);
        }

        public class MultipleCommand1 : Command
        {
        }

        public class MultipleCommand2 : Command
        {
            
        }

        public class MultipleCommandCommandHandler : CommandHandler, ICommandHandler<MultipleCommand1, string>, ICommandHandler<MultipleCommand2, string>
        {
            private readonly ISpy spy;

            public MultipleCommandCommandHandler(ISpy spy)
            {
                this.spy = spy;
            }

            public string Handle(MultipleCommand1 command, CommandHandlerContext context)
            {
                this.spy.Spy("MultipleCommand1");
                return string.Empty;
            }

            public string Handle(MultipleCommand2 command, CommandHandlerContext context)
            {
                this.spy.Spy("MultipleCommand2");
                return string.Empty;
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