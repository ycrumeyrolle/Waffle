namespace CommandProcessing.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
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
    public sealed class CommandProcessorFixture : IDisposable
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
        public void WhenProcessingMultipleCommandThenCommandsAreProcessed()
        {
            // Arrange
            Mock<ISpy> spy = new Mock<ISpy>(MockBehavior.Strict);
            spy.Setup(s => s.Spy("MultipleCommand1"));
            spy.Setup(s => s.Spy("MultipleCommand2"));
            MultipleHandler handler = new MultipleHandler(spy.Object);
            this.SetupHandler<MultipleCommand1, string>(handler);
            this.SetupHandler<MultipleCommand2, string>(handler);
            CommandProcessor processor = this.CreatTestableProcessor();
            MultipleCommand1 command1 = new MultipleCommand1();
            MultipleCommand2 command2 = new MultipleCommand2();

            // Act & assert
            processor.Process<MultipleCommand1, string>(command1);
            spy.Verify(h => h.Spy("MultipleCommand1"), Times.Once());
            spy.Verify(h => h.Spy("MultipleCommand2"), Times.Never());
            processor.Process<MultipleCommand2, string>(command2);

            spy.Verify(h => h.Spy("MultipleCommand1"), Times.Once());
            spy.Verify(h => h.Spy("MultipleCommand2"), Times.Once());
        }
        
        [TestMethod]
        public void WhenProcessingThrowExceptionWithoutFilterThenThrowsException()
        {
            // Arrange
            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>(null);
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
            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>(null);
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

            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>(config);

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

            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>(null);
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

            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>(null);

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


            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>();
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
            HandlerDescriptor descriptor = new HandlerDescriptor(config, typeof(TCommand), handler.Object.GetType());
            selector
                .Setup(s => s.SelectHandler(It.IsAny<HandlerRequest>()))
                .Returns(descriptor);

            config.Services.Replace(typeof(IHandlerSelector), selector.Object);


            return handler;
        }

        private Handler SetupHandlerImpl<TCommand, TResult>(Handler handler) where TCommand : ICommand
        {
            ProcessorConfiguration config = this.configuration;
            Mock<IHandlerActivator> activator = new Mock<IHandlerActivator>(MockBehavior.Strict);
            activator
                .Setup(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()))
                .Returns(handler);
            config.Services.Replace(typeof(IHandlerActivator), activator.Object);

            Mock<IHandlerSelector> selector = new Mock<IHandlerSelector>(MockBehavior.Strict);
            HandlerDescriptor descriptor = new HandlerDescriptor(config, typeof(TCommand), handler.GetType());
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

        private Mock<Handler<TCommand, TResult>> SetupThrowingHandler<TCommand, TResult, TException>(ProcessorConfiguration config = null)
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

        private Mock<Handler<TCommand, TResult>> SetupHandler<TCommand, TResult>(ProcessorConfiguration config = null, Func<TResult> funcResult = null)
            where TCommand : ICommand
            where TResult : class
        {
            Mock<Handler<TCommand, TResult>> handler = this.SetupHandlerImpl<TCommand, TResult>(config);
            handler
                .Setup(h => h.Handle(It.IsAny<TCommand>()))
                .Returns(funcResult);
            return handler;
        }

        private Mock<Handler<TCommand, VoidResult>> SetupHandler<TCommand>(ProcessorConfiguration config = null)
            where TCommand : ICommand
        {
            Mock<Handler<TCommand, VoidResult>> handler = this.SetupHandlerImpl<TCommand, VoidResult>(config);
            handler.Setup(h => h.Handle(It.IsAny<TCommand>()));
            return handler;
        }
        
        private IHandler<TCommand, TResult> SetupHandler<TCommand, TResult>(Handler handler)
            where TCommand : ICommand
        {
            return (IHandler<TCommand, TResult>)this.SetupHandlerImpl<TCommand, TResult>(handler);
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
            HandlerDescriptor descriptor = new HandlerDescriptor(this.configuration, typeof(ValidCommand), typeof(ValidHandler));
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
        public void WhenProcessingDifferentsCommandsWithCacheVaryByAllThenReturnNewValue()
        {
            Mock<IHandlerActivator> activator = new Mock<IHandlerActivator>(MockBehavior.Strict);
            ValidCommand command1 = new ValidCommand();
            command1.Property = "test1";
            ValidCommand command2 = new ValidCommand();
            command2.Property = "test2";
            activator.Setup(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()));
            this.configuration.Services.Replace(typeof(IHandlerActivator), activator.Object);
            int i = 0;
            this.SetupHandler<ValidCommand, string>(null, () => (i++).ToString(CultureInfo.InvariantCulture));

            CommandProcessor processor = this.CreatTestableProcessor();

            CacheAttribute attribute = new CacheAttribute();
            attribute.Duration = 10;
            attribute.VaryByParams = CacheAttribute.VaryByParamsAll;
            this.configuration.Filters.Add(attribute);

            // Act
            var result1 = processor.Process<ValidCommand, string>(command1);
            var result2 = processor.Process<ValidCommand, string>(command2);

            Assert.AreNotEqual(result1, result2);
        }

        [TestMethod]
        public void WhenProcessingDifferentsCommandsWithCacheVaryByNoneThenReturnValueInCache()
        {
            Mock<IHandlerActivator> activator = new Mock<IHandlerActivator>(MockBehavior.Strict);
            ValidCommand command1 = new ValidCommand();
            command1.Property = "test1";
            ValidCommand command2 = new ValidCommand();
            command2.Property = "test2";
            activator.Setup(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()));
            this.configuration.Services.Replace(typeof(IHandlerActivator), activator.Object);
            int i = 0;
            this.SetupHandler<ValidCommand, string>(null, () => (i++).ToString(CultureInfo.InvariantCulture));

            CommandProcessor processor = this.CreatTestableProcessor();

            CacheAttribute attribute = new CacheAttribute();
            attribute.Duration = 10;
            attribute.VaryByParams = CacheAttribute.VaryByParamsNone;
            this.configuration.Filters.Add(attribute);

            // Act
            var result1 = processor.Process<ValidCommand, string>(command1);
            var result2 = processor.Process<ValidCommand, string>(command2);

            // Without cache result1 would be different from result2
            // With the cache results are the same
            Assert.AreEqual(result1, result2);
        }

        [TestMethod]
        public void WhenProcessingCyclicCommandWithCacheThenReturnValueInCache()
        {
            Mock<IHandlerActivator> activator = new Mock<IHandlerActivator>(MockBehavior.Strict);
            ComplexCyclicCommand command1 = new ComplexCyclicCommand();
            ComplexCyclicCommand command2 = new ComplexCyclicCommand();
            activator.Setup(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()));
            this.configuration.Services.Replace(typeof(IHandlerActivator), activator.Object);

            int i = 0;
            this.SetupHandler<ComplexCyclicCommand, string>(null, () => (i++).ToString(CultureInfo.InvariantCulture));

            CommandProcessor processor = this.CreatTestableProcessor();

            CacheAttribute attribute = new CacheAttribute();
            attribute.Duration = 10;
            attribute.VaryByParams = CacheAttribute.VaryByParamsAll;
            this.configuration.Filters.Add(attribute);

            // Act
            var result1 = processor.Process<ComplexCyclicCommand, string>(command1);
            var result2 = processor.Process<ComplexCyclicCommand, string>(command2);

            // Without cache result 1 would be different from result2
            // With the cache results are the same
            Assert.AreEqual(result1, result2);
        }


        [TestMethod]
        public void WhenProcessingDifferentCyclicCommandsWithCacheThenReturnValueInCacheBecauseItIsNotManaged()
        {
            Mock<IHandlerActivator> activator = new Mock<IHandlerActivator>(MockBehavior.Strict);
            ComplexCyclicCommand command1 = new ComplexCyclicCommand();
            ComplexCyclicCommand command2 = new ComplexCyclicCommand();
            command2.Property2 = command1.Property2;
            activator.Setup(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()));
            this.configuration.Services.Replace(typeof(IHandlerActivator), activator.Object);

            int i = 0;
            this.SetupHandler<ComplexCyclicCommand, string>(null, () => (i++).ToString(CultureInfo.InvariantCulture));

            CommandProcessor processor = this.CreatTestableProcessor();

            CacheAttribute attribute = new CacheAttribute();
            attribute.Duration = 10;
            attribute.VaryByParams = CacheAttribute.VaryByParamsAll;
            this.configuration.Filters.Add(attribute);

            // Act
            var result1 = processor.Process<ComplexCyclicCommand, string>(command1);
            var result2 = processor.Process<ComplexCyclicCommand, string>(command2);

            // Without cache result 1 would be different from result2
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
                config.EnableDefaultTracing();
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

        public class MultipleHandler : Handler, IHandler<MultipleCommand1, string>, IHandler<MultipleCommand2, string>
        {
            private readonly ISpy spy;

            public MultipleHandler(ISpy spy)
            {
                this.spy = spy;
            }

            public string Handle(MultipleCommand1 command)
            {
                this.spy.Spy("MultipleCommand1");
                return string.Empty;
            }

            public string Handle(MultipleCommand2 command)
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