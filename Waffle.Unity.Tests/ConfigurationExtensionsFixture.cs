namespace Waffle.Unity.Tests
{
    using Microsoft.Practices.Unity;
    using Moq;
    using System;
    using System.Collections.Generic;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;
    using Waffle.Validation;
    using Xunit;
    
    public sealed class ConfigurationExtensionsFixture : IDisposable
    {
        private readonly ProcessorConfiguration configuration = new ProcessorConfiguration();

        public ConfigurationExtensionsFixture()
        {
            this.configuration = new ProcessorConfiguration();
        }

        [Fact]
        public void WhenRegisteringNullConfigurationThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => ConfigurationExtensions.RegisterContainer(null, null), "configuration");
        }

        [Fact]
        public void WhenRegisteringContainerThenReturnsResolver()
        {
            // Arrange
            Mock<IUnityContainer> container = new Mock<IUnityContainer>();

            // Act
            var resolver = this.configuration.RegisterContainer(container.Object);

            // Assert
            Assert.NotNull(resolver);
        }
                
        [Fact]
        public void RegisterHandlers_()
        {
            Mock<IUnityContainer> container = new Mock<IUnityContainer>();

            Dictionary<Type, CommandHandlerDescriptor> commands = new Dictionary<Type, CommandHandlerDescriptor>
                                                                  {
                                                                      { typeof(object), new CommandHandlerDescriptor(this.configuration, typeof(FakeCommand), typeof(FakeTransientHandler)) }
                                                                  };

            Mock<ICommandHandlerDescriptorProvider> commandDescProvider = new Mock<ICommandHandlerDescriptorProvider>();
            commandDescProvider
                .Setup(p => p.GetHandlerMapping())
                .Returns(commands);

            List<EventHandlerDescriptor> eventDescriptors = new List<EventHandlerDescriptor>();
            eventDescriptors.Add(new EventHandlerDescriptor(this.configuration, typeof(FakeEvent), typeof(FakeTransientHandler)));
            eventDescriptors.Add(new EventHandlerDescriptor(this.configuration, typeof(FakeEvent), typeof(FakePerRequestHandler)));
            eventDescriptors.Add(new EventHandlerDescriptor(this.configuration, typeof(FakeEvent), typeof(FakeProcessorHandler)));

            Dictionary<Type, EventHandlersDescriptor> events = new Dictionary<Type, EventHandlersDescriptor> { { typeof(FakeEvent), new EventHandlersDescriptor("xx", eventDescriptors) } };

            Mock<IEventHandlerDescriptorProvider> eventDescProvider = new Mock<IEventHandlerDescriptorProvider>();
            eventDescProvider
                .Setup(p => p.GetHandlerMapping())
                .Returns(events);

            this.configuration.Services.Replace(typeof(ICommandHandlerDescriptorProvider), commandDescProvider.Object);
            this.configuration.Services.Replace(typeof(IEventHandlerDescriptorProvider), eventDescProvider.Object);
            this.configuration.RegisterHandlers(container.Object);
        }
        
        public void Dispose()
        {
            this.configuration.Dispose();
        }
        
        private class FakeCommand : ICommand
        {
            /// <summary>
            /// Gets whether the command is valid.
            /// </summary>
            /// <value><c>true</c> id the command is valid ; <c>false</c> otherwise.</value>
            public bool IsValid { get; private set; }

            /// <summary>
            /// Gets the validation results collection.
            /// </summary>
            /// <value>The validation results collection.</value>
            public ModelStateDictionary ModelState { get; private set; }
        }

        private class FakeEvent : IEvent
        {
            /// <summary>
            /// Gets the identifier of the source originating the event.
            /// </summary>
            /// <value>The identifier of the source originating the event.</value>
            public Guid SourceId { get; private set; }
        }

        [HandlerLifetime(HandlerLifetime.Transient)]
        private class FakeTransientHandler : MessageHandler,
            IEventHandler<FakeEvent>,
            ICommandHandler<FakeCommand>
        {
            /// <summary>
            /// Handle the command.
            /// </summary>
            /// <param name="command">The <see cref="ICommand"/> to process.</param>
            /// <returns>The result object.</returns>
            public void Handle(FakeCommand command)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Handle the event.
            /// </summary>
            /// <param name="event">The <see cref="IEvent"/> to handle.</param>
            public void Handle(FakeEvent @event)
            {
                throw new NotImplementedException();
            }
        }

        [HandlerLifetime(HandlerLifetime.Singleton)]
        private class FakeProcessorHandler : MessageHandler,
            IEventHandler<FakeEvent>
        {
            /// <summary>
            /// Handle the event.
            /// </summary>
            /// <param name="event">The <see cref="IEvent"/> to handle.</param>
            public void Handle(FakeEvent @event)
            {
                throw new NotImplementedException();
            }
        }

        [HandlerLifetime(HandlerLifetime.PerRequest)]
        private class FakePerRequestHandler : MessageHandler,
            IEventHandler<FakeEvent>
        {
            /// <summary>
            /// Handle the event.
            /// </summary>
            /// <param name="event">The <see cref="IEvent"/> to handle.</param
            public void Handle(FakeEvent @event)
            {
                throw new NotImplementedException();
            }
        }
    }
}