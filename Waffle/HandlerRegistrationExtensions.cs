namespace Waffle
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Internal;

    /// <summary>
    /// Provides extension methods for the handlers.
    /// </summary>
    public static class HandlerRegistrationExtensions
    {
        /// <summary>
        /// Registers a dynamic command handler.
        /// </summary>
        /// <typeparam name="TCommand">The type of the <see cref="ICommand"/>.</typeparam>
        /// <param name="config">The configuration.</param>
        /// <param name="handler">The <see cref="Func{TCommand, Task}"/> delegate.</param>
        public static void RegisterCommandHandler<TCommand>(this ProcessorConfiguration config, Func<TCommand, Task> handler) where TCommand : ICommand
        {
            if (config == null)
            {
                Error.ArgumentNull("config");
            }

            if (handler == null)
            {
                Error.ArgumentNull("handler");
            }

            RegisterCommandHandlerCore(config, handler);
        }

        /// <summary>
        /// Registers a dynamic command handler.
        /// </summary>
        /// <typeparam name="TCommand">The type of the <see cref="ICommand"/>.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="config">The configuration.</param>
        /// <param name="handler">The <see cref="Func{TCommand, Task}"/> delegate.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for Task<TResult> pattern.")]
        public static void RegisterCommandHandler<TCommand, TResult>(this ProcessorConfiguration config, Func<TCommand, Task<TResult>> handler) where TCommand : ICommand
        {
            if (config == null)
            {
                Error.ArgumentNull("config");
            }

            if (handler == null)
            {
                Error.ArgumentNull("handler");
            }

            RegisterCommandHandlerCore(config, handler);
        }

        private static void RegisterCommandHandlerCore<TCommand>(ProcessorConfiguration config, Func<TCommand, Task> handler) where TCommand : ICommand
        {
            Contract.Requires(config != null);

            ICommandHandlerDescriptorProvider descriptorProvider = config.Services.GetCommandHandlerDescriptorProvider();
            IDictionary<Type, CommandHandlerDescriptor> mapping = descriptorProvider.GetHandlerMapping();
            mapping.Add(typeof(TCommand), new CommandHandlerDescriptor<TCommand>(config, typeof(TCommand), handler));
        }

        /// <summary>
        /// Registers a dynamic event handler.
        /// </summary>
        /// <typeparam name="TEvent">The type of the <see cref="IEvent"/>.</typeparam>
        /// <param name="config">The configuration.</param>
        /// <param name="handler">The <see cref="Func{TEvent, Task}"/> delegate.</param>
        public static void RegisterEventHandler<TEvent>(this ProcessorConfiguration config, Func<TEvent, Task> handler) where TEvent : IEvent
        {
            if (config == null)
            {
               throw Error.ArgumentNull("config");
            }

            if (handler == null)
            {
                throw Error.ArgumentNull("handler");
            }

            var descriptorProvider = config.Services.GetEventHandlerDescriptorProvider();
            var mapping = descriptorProvider.GetHandlerMapping();
            Type eventType = typeof(TEvent);
            EventHandlersDescriptor descriptors;
            if (!mapping.TryGetValue(eventType, out descriptors)) 
            {
                descriptors = new EventHandlersDescriptor(eventType.Name);
                mapping.Add(typeof(TEvent), descriptors);
            }

            descriptors.EventHandlerDescriptors.Add(new EventHandlerDescriptor<TEvent>(config, eventType, handler));
        }
    }
}