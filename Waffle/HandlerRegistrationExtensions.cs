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

    public static class HandlerRegistrationExtensions
    {
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