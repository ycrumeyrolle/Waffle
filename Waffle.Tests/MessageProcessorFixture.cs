namespace Waffle.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Runtime.ExceptionServices;
    using Moq;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Dependencies;
    using Waffle.Filters;
    using Waffle.Retrying;
    using Waffle.Tests.Helpers;
    using System.Threading.Tasks;
    using Xunit;
    using System.Threading;
    using Waffle.Tasks;

    public sealed class MessageProcessorFixture : IDisposable
    {
        private readonly ICollection<IDisposable> disposableResources = new Collection<IDisposable>();

        private readonly ProcessorConfiguration configuration = new ProcessorConfiguration();

        private readonly Mock<IDependencyResolver> dependencyResolver = new Mock<IDependencyResolver>(MockBehavior.Loose);

        [Fact]
        public void WhenCreatingProcessorWithoutConfigThenHasDefaultConfig()
        {
            // Act
            MessageProcessor processor = new MessageProcessor();
            this.disposableResources.Add(processor);

            // Assert
            Assert.NotNull(processor.Configuration);
        }

        [Fact]
        public void WhenCreatingProcessorWithNullConfigThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => new MessageProcessor(null), "configuration");
        }

        [Fact]
        public void WhenCreatingProcessorWithCustomConfigThenHasCustomConfig()
        {
            // Arrange
            ProcessorConfiguration config = new ProcessorConfiguration();

            // Act
            MessageProcessor processor = this.CreatTestableProcessor(config);
            this.disposableResources.Add(processor);

            // Assert
            Assert.NotNull(processor.Configuration);
            Assert.Same(config, processor.Configuration);
        }

        [Fact]
        public async Task WhenProcessingValidCommandThenCommandIsProcessed()
        {
            // Arrange
            var handler = this.SetupHandler<ValidCommand, string>(null, "OK");
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = await processor.ProcessAsync<string>(command);

            // Assert
            Assert.Equal("OK", result.Value);
            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
        }

        [Fact]
        public async Task WhenProcessingCommandWithoutResultThenCommandIsProcessed()
        {
            // Arrange
            var handler = this.SetupHandler<ValidCommand, string>(null, "OK");
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            await processor.ProcessAsync(command);

            // Assert
            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
        }

        [Fact]
        public async Task WhenProcessingMultipleCommandThenCommandsAreProcessed()
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
            await processor.ProcessAsync<string>(command1);
            spy.Verify(h => h.Spy("MultipleCommand1"), Times.Once());
            spy.Verify(h => h.Spy("MultipleCommand2"), Times.Never());

            // Handler must be configured now to avoid incorrect descriptor creation
            this.SetupHandler<MultipleCommand2, string>(commandCommandHandler);
            await processor.ProcessAsync<string>(command2);

            spy.Verify(h => h.Spy("MultipleCommand1"), Times.Once());
            spy.Verify(h => h.Spy("MultipleCommand2"), Times.Once());

            await processor.ProcessAsync<string>(command2);
            spy.Verify(h => h.Spy("MultipleCommand1"), Times.Once());
            spy.Verify(h => h.Spy("MultipleCommand2"), Times.Exactly(2));
        }

        [Fact]
        public void WhenProcessingThrowExceptionWithoutFilterThenThrowsException()
        {
            // Arrange
            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>();
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            Assert.ThrowsAsync<Exception>(async () => await processor.ProcessAsync(command));
            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
        }

        [Fact]
        public void WhenProcessingThrowExceptionWithUselessFilterThenThrowsException()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> exceptionFilter = new Mock<ExceptionFilterAttribute>(MockBehavior.Strict);
            exceptionFilter.Setup(f => f.OnException(It.IsAny<CommandHandlerExecutedContext>()));
            this.configuration.Filters.Add(exceptionFilter.Object);
            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>();
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            Assert.ThrowsAsync<Exception>(async () => await processor.ProcessAsync(command));
            exceptionFilter.Verify(f => f.OnException(It.IsAny<CommandHandlerExecutedContext>()), Times.Once());
            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
        }

        [Fact]
        public async Task WhenProcessingThrowExceptionWithHandlingFilterThenReturnsExceptionValue()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> exceptionFilter = new Mock<ExceptionFilterAttribute>(MockBehavior.Strict);
            exceptionFilter
                .Setup(f => f.OnException(It.IsAny<CommandHandlerExecutedContext>()))
                .Callback<CommandHandlerExecutedContext>(c => c.Response = c.Request.CreateResponse("Exception !!"));
            ProcessorConfiguration config = new ProcessorConfiguration();
            config.Filters.Add(exceptionFilter.Object);

            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>(config);

            MessageProcessor processor = this.CreatTestableProcessor(config);
            ValidCommand command = new ValidCommand();

            // Act
            var result = await processor.ProcessAsync<string>(command);

            exceptionFilter.Verify(f => f.OnException(It.IsAny<CommandHandlerExecutedContext>()), Times.Once());
            Assert.Equal("Exception !!", result.Value);

            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
        }

        [Fact]
        public async Task WhenProcessingThrowExceptionWithHandlingFilterAndResultThenReturnsExceptionValue()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> exceptionFilter = new Mock<ExceptionFilterAttribute>(MockBehavior.Strict);
            exceptionFilter
                .Setup(f => f.OnException(It.IsAny<CommandHandlerExecutedContext>()))
                .Callback<CommandHandlerExecutedContext>(c => c.Response = c.Request.CreateResponse("Exception !!"));
            this.configuration.Filters.Add(exceptionFilter.Object);

            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>();
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = await processor.ProcessAsync<string>(command);

            Assert.Equal("Exception !!", result.Value);
            exceptionFilter.Verify(f => f.OnException(It.IsAny<CommandHandlerExecutedContext>()), Times.Once());

            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
        }

        [Fact]
        public async Task WhenProcessingWithFiltersThenReturnsValueTransformedByFilters()
        {
            // Arrange
            var filter1 = this.SetupFilter("X");
            var filter2 = this.SetupFilter("Y");

            var handler = this.SetupHandler<ValidCommand, string>(null, "OK");
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = await processor.ProcessAsync<string>(command);

            Assert.NotNull(result.Value);
            Assert.True(result.Value.Contains("OK"));
            Assert.True(result.Value.Contains("X"));
            Assert.True(result.Value.Contains("Y"));

            filter1.Verify(f => f.OnCommandExecutingAsync(It.IsAny<CommandHandlerContext>(), It.IsAny<CancellationToken>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecutedAsync(It.IsAny<CommandHandlerExecutedContext>(), It.IsAny<CancellationToken>()), Times.Once());

            filter2.Verify(f => f.OnCommandExecutingAsync(It.IsAny<CommandHandlerContext>(), It.IsAny<CancellationToken>()), Times.Once());
            filter2.Verify(f => f.OnCommandExecutedAsync(It.IsAny<CommandHandlerExecutedContext>(), It.IsAny<CancellationToken>()), Times.Once());

            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
        }

        [Fact]
        public async Task WhenProcessingWithFilterSettingValueThenReturnsValueFromFilter()
        {
            // Arrange
            var filter1 = this.SetupFilter("X", (c, t) => 
            {
                c.Response = c.Request.CreateResponse("OK from filter");
                return TaskHelpers.Completed();
            });

            var handler = this.SetupHandler<ValidCommand, string>(null, "OK");
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = await processor.ProcessAsync<string>(command);

            Assert.Equal("OK from filter", result.Value);
            filter1.Verify(f => f.OnCommandExecutingAsync(It.IsAny<CommandHandlerContext>(), It.IsAny<CancellationToken>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecutedAsync(It.IsAny<CommandHandlerExecutedContext>(), It.IsAny<CancellationToken>()), Times.Never());

            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Never());
        }

        [Fact]
        public async Task WhenProcessingWithFiltersThrowExceptionThenHandlesExceptionByExceptionFilter()
        {
            // Arrange
            this.configuration.Filters.Clear();
            Mock<ExceptionFilterAttribute> exceptionFilter = new Mock<ExceptionFilterAttribute>(MockBehavior.Strict);
            exceptionFilter
                .Setup(f => f.OnException(It.IsAny<CommandHandlerExecutedContext>()))
                .Callback<CommandHandlerExecutedContext>(c => c.Response = c.Request.CreateResponse("Exception !!"));
            this.configuration.Filters.Add(exceptionFilter.Object);

            var filter1 = this.SetupFilter("X");
            var filter2 = this.SetupFilter("Y");

            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>();

            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = await processor.ProcessAsync<string>(command);

            Assert.Equal("Exception !!", result.Value);
            filter1.Verify(f => f.OnCommandExecutingAsync(It.IsAny<CommandHandlerContext>(), It.IsAny<CancellationToken>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecutedAsync(It.IsAny<CommandHandlerExecutedContext>(), It.IsAny<CancellationToken>()), Times.Once());

            filter2.Verify(f => f.OnCommandExecutingAsync(It.IsAny<CommandHandlerContext>(), It.IsAny<CancellationToken>()), Times.Once());
            filter2.Verify(f => f.OnCommandExecutedAsync(It.IsAny<CommandHandlerExecutedContext>(), It.IsAny<CancellationToken>()), Times.Once());

            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
        }

        [Fact]
        public async Task WhenProcessingWithFiltersThrowExceptionThenHandlesExceptionByHandlerFilter()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> exceptionFilter = new Mock<ExceptionFilterAttribute>(MockBehavior.Strict);
            exceptionFilter
                .Setup(f => f.OnException(It.IsAny<CommandHandlerExecutedContext>()))
                .Callback<CommandHandlerExecutedContext>(c => c.Response = c.Request.CreateResponse("Exception !!"));
            this.configuration.Filters.Add(exceptionFilter.Object);

            var filter1 = this.SetupFilter("X");
            var filter2 = this.SetupFilter("Y");
            var filter3 = this.SetupFilter("Z");


            var handler = this.SetupThrowingHandler<ValidCommand, string, Exception>();
            MessageProcessor processor = this.CreatTestableProcessor();
            ValidCommand command = new ValidCommand();

            // Act
            var result = await processor.ProcessAsync<string>(command);

            // Assert
            // Exception filter overrides result of handler filters
            Assert.Equal("Exception !!", result.Value);
            filter1.Verify(f => f.OnCommandExecutingAsync(It.IsAny<CommandHandlerContext>(), It.IsAny<CancellationToken>()), Times.Once());
            filter1.Verify(f => f.OnCommandExecutedAsync(It.IsAny<CommandHandlerExecutedContext>(), It.IsAny<CancellationToken>()), Times.Once());

            filter2.Verify(f => f.OnCommandExecutingAsync(It.IsAny<CommandHandlerContext>(), It.IsAny<CancellationToken>()), Times.Once());
            filter2.Verify(f => f.OnCommandExecutedAsync(It.IsAny<CommandHandlerExecutedContext>(), It.IsAny<CancellationToken>()), Times.Once());

            filter3.Verify(f => f.OnCommandExecutingAsync(It.IsAny<CommandHandlerContext>(), It.IsAny<CancellationToken>()), Times.Once());
            filter3.Verify(f => f.OnCommandExecutedAsync(It.IsAny<CommandHandlerExecutedContext>(), It.IsAny<CancellationToken>()), Times.Once());

            handler.Verify(h => h.Handle(It.IsAny<ValidCommand>()), Times.Once());
        }

        private Mock<CommandHandlerFilterAttribute> SetupFilter(string value, Func<CommandHandlerContext, CancellationToken, Task> executingCallback = null, Func<CommandHandlerExecutedContext, CancellationToken, Task> executedCallback = null)
        {
            executingCallback = executingCallback ?? ((_, t) => TaskHelpers.Completed());
            executedCallback = executedCallback ?? ((_, t) => TaskHelpers.Completed());
            Mock<CommandHandlerFilterAttribute> filter = new Mock<CommandHandlerFilterAttribute>(MockBehavior.Strict);
            filter.Setup(f => f.AllowMultiple).Returns(true);          
            filter
                .Setup(f => f.OnCommandExecutingAsync(It.IsAny<CommandHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns<CommandHandlerContext, CancellationToken>((c, t) => executingCallback(c, t));
            filter
                .Setup(f => f.OnCommandExecutedAsync(It.IsAny<CommandHandlerExecutedContext>(), It.IsAny<CancellationToken>()))
                .Returns<CommandHandlerExecutedContext, CancellationToken>((c, t) =>
                {
                    if (c.ExceptionInfo == null)
                    {
                        var previous = c.Response != null ? c.Response.Value : string.Empty;
                        c.Response = new HandlerResponse(c.Request, previous + value);
                    }

                    return executedCallback(c, t);
                });
            this.configuration.Filters.Add(filter.Object);
            return filter;
        }

        private Mock<ICommandHandler<TCommand, TResult>> SetupHandlerImpl<TCommand, TResult>(ProcessorConfiguration config = null) where TCommand : ICommand
        {
            config = config ?? this.configuration;
            Mock<ICommandHandler<TCommand, TResult>> handler = new Mock<ICommandHandler<TCommand, TResult>>(MockBehavior.Strict);
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

        private MessageHandler SetupHandlerImpl<TCommand, TResult>(MessageHandler commandHandler) where TCommand : ICommand
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
                .Setup(h => h.Handle(It.IsAny<TCommand>()))
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
                .Setup(h => h.Handle(It.IsAny<TCommand>()))
                .Throws<TException>();
            handler.SetupAllProperties();
            return handler;
        }

        private Mock<ICommandHandler<TCommand, TResult>> SetupHandler<TCommand, TResult>(ProcessorConfiguration config = null, TResult result = null)
            where TCommand : ICommand
            where TResult : class
        {
            Mock<ICommandHandler<TCommand, TResult>> handler = this.SetupHandlerImpl<TCommand, TResult>(config);
            handler
                .Setup(h => h.Handle(It.IsAny<TCommand>()))
                .Returns(result);
            handler.SetupAllProperties();
            return handler;
        }

        private Mock<ICommandHandler<TCommand, TResult>> SetupHandler<TCommand, TResult>(ProcessorConfiguration config = null, Func<TResult> funcResult = null)
            where TCommand : ICommand
            where TResult : class
        {
            Mock<ICommandHandler<TCommand, TResult>> handler = this.SetupHandlerImpl<TCommand, TResult>(config);
            handler
                .Setup(h => h.Handle(It.IsAny<TCommand>()))
                .Returns(funcResult());
            handler.SetupAllProperties();
            return handler;
        }

        private Mock<ICommandHandler<TCommand, VoidResult>> SetupHandler<TCommand>(ProcessorConfiguration config = null)
            where TCommand : ICommand
        {
            Mock<ICommandHandler<TCommand, VoidResult>> handler = this.SetupHandlerImpl<TCommand, VoidResult>(config);
            handler.Setup(h => h.Handle(It.IsAny<TCommand>()));
            return handler;
        }

        private MessageHandler SetupHandler<TCommand, TResult>(MessageHandler commandHandler)
            where TCommand : ICommand
        {
            return this.SetupHandlerImpl<TCommand, TResult>(commandHandler);
        }

        [Fact]
        public void WhenProcessingUnknowHandlerThenThrowsHandlerNotFoundException()
        {
            // Arrange
            Mock<ICommandHandlerActivator> activator = new Mock<ICommandHandlerActivator>(MockBehavior.Strict);
            activator
                .Setup(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()))
                .Returns<MessageHandler>(null);
            this.configuration.Services.Replace(typeof(ICommandHandlerActivator), activator.Object);

            Mock<ICommandHandlerSelector> selector = new Mock<ICommandHandlerSelector>(MockBehavior.Strict);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.configuration, typeof(ValidCommand), typeof(ValidCommandHandler));
            selector.Setup(s => s.SelectHandler(It.IsAny<CommandHandlerRequest>())).Returns(descriptor);

            this.configuration.Services.Replace(typeof(ICommandHandlerSelector), selector.Object);
            ValidCommand command = new ValidCommand();
            MessageProcessor processor = this.CreatTestableProcessor();

            // Act
            Assert.ThrowsAsync<CommandHandlerNotFoundException>(async () => await processor.ProcessAsync(command));

            // Assert
            activator.Verify(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()), Times.Once());
            selector.Verify(a => a.SelectHandler(It.IsAny<CommandHandlerRequest>()), Times.Once());
        }

        [Fact]
        public async Task WhenProcessingInvalidCommandThenAbortProcesssing()
        {
            // Arrange
            InvalidCommand command = new InvalidCommand();

            var handler = this.SetupHandler<InvalidCommand, string>(null, "OK");

            MessageProcessor processor = this.CreatTestableProcessor();

            // Act
            var result = await processor.ProcessAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(0, result.ModelState.Count);
            handler.Verify(h => h.Handle(It.IsAny<InvalidCommand>()), Times.Never());
        }

        [Fact]
        public void WhenProcessingRetringCommandButAlwayFailsThenThrowsInvalidOperationException()
        {
            // Arrange
            Mock<ISpy> spy = new Mock<ISpy>(MockBehavior.Strict);
            spy.Setup(s => s.Spy("Trying"));
            RetryHandler commandCommandHandler = new RetryHandler(spy.Object);
            this.SetupHandler<RetryCommand, object>(commandCommandHandler);

            MessageProcessor processor = this.CreatTestableProcessor();
            RetryCommand command = new RetryCommand(10);

            // Act
            Assert.ThrowsAsync<InvalidOperationException>(async () => await processor.ProcessAsync(command));

            // Assert
            spy.Verify(s => s.Spy("Trying"), Times.Exactly(6));
        }

        [Fact]
        public async Task WhenProcessingRetringCommandThenReturnResult()
        {
            // Arrange
            Mock<ISpy> spy = new Mock<ISpy>(MockBehavior.Strict);
            spy.Setup(s => s.Spy("Trying"));
            RetryHandler commandCommandHandler = new RetryHandler(spy.Object);
            this.SetupHandler<RetryCommand, string>(commandCommandHandler);

            MessageProcessor processor = this.CreatTestableProcessor();
            RetryCommand command = new RetryCommand(2);

            // Act
            var result = await processor.ProcessAsync<string>(command);

            // Assert
            Assert.Equal("OK", result.Value);
            spy.Verify(s => s.Spy("Trying"), Times.Exactly(3));
        }

        [Fact]
        public void WhenUsingUnknowServiceThenThrowsArgumentException()
        {
            // Arrange
            MessageProcessor processor = this.CreatTestableProcessor();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => processor.Use<ISimpleService>());
        }

        [Fact]
        public void WhenUsingServiceWithProxyThenReturnsServiceProxy()
        {
            // Arrange
            SimpleService service = new SimpleService();
            this.dependencyResolver.Setup(d => d.GetService(typeof(ISimpleService))).Returns(service);
            this.configuration.ServiceProxyCreationEnabled = true;

            MessageProcessor processor = this.CreatTestableProcessor();

            // Act
            var serviceProxy = processor.Use<ISimpleService>();

            // Assert
            Assert.NotNull(serviceProxy);
            Assert.IsAssignableFrom(service.GetType(), serviceProxy);
            Assert.False(serviceProxy.GetType() == service.GetType());
        }

        [Fact]
        public void WhenUsingServiceWithoutProxyThenReturnsService()
        {
            // Arrange
            SimpleService service = new SimpleService();
            this.dependencyResolver.Setup(d => d.GetService(typeof(ISimpleService))).Returns(service);
            this.configuration.ServiceProxyCreationEnabled = false;

            MessageProcessor processor = this.CreatTestableProcessor();

            // Act
            var serviceDirect = processor.Use<ISimpleService>();

            // Assert
            Assert.NotNull(serviceDirect);
            Assert.IsType(service.GetType(), serviceDirect);
            Assert.True(serviceDirect.GetType() == service.GetType());
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

        public class ValidCommandHandler : MessageHandler, ICommandHandler<ValidCommand, string>
        {
            public string Handle(ValidCommand command)
            {
                return command.Property;
            }
        }

        public class ValidCommandHandlerWithoutResult : MessageHandler, ICommandHandler<ValidCommand>
        {
            public void Handle(ValidCommand command)
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

        public class CyclicCommand : ICommand
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

        public class ComplexCyclicCommand : ICommand
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

        public class MultipleCommand1 : ICommand
        {
        }

        public class MultipleCommand2 : ICommand
        {

        }

        public class MultipleCommandCommandHandler : MessageHandler, ICommandHandler<MultipleCommand1, string>, ICommandHandler<MultipleCommand2, string>
        {
            private readonly ISpy spy;

            public MultipleCommandCommandHandler(ISpy spy)
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

        public class RetryCommand : ICommand
        {
            public RetryCommand(int retryCount)
            {
                this.RetryCount = retryCount;
            }

            public int RetryCount { get; set; }
        }

        [Retry(5, 10.0)]
        public class RetryHandler : MessageHandler, ICommandHandler<RetryCommand, string>
        {
            private readonly ISpy spy;

            public RetryHandler(ISpy spy)
            {
                this.spy = spy;
            }

            public string Handle(RetryCommand command)
            {
                this.spy.Spy("Trying");
                if (command.RetryCount-- > 0)
                {
                    throw new InvalidOperationException("Try again !");
                }

                return "OK";
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