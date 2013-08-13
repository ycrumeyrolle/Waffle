﻿namespace Waffle
{
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Dependencies;
    using Waffle.Events;

    internal class MessageProcessorWrapper : IMessageProcessor
    {
        private readonly MessageProcessor inner;

        private readonly HandlerRequest request;

        public MessageProcessorWrapper(MessageProcessor inner, HandlerRequest request)
        {
            this.inner = inner;
            this.request = request;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public ProcessorConfiguration Configuration
        {
            get { return this.inner.Configuration; }
        }

        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="command">The command to process.</param>
        /// <returns>The result of the command.</returns>
        public Task<TResult> ProcessAsync<TResult>(ICommand command)
        {
            return this.inner.ProcessAsync<TResult>(command, this.request);
        }

        /// <summary>
        /// Publish the event. 
        /// </summary>
        /// <param name="event">The event to publish.</param>
        /// <returns>The <see cref="Task"/> of the event.</returns>
        public Task PublishAsync(IEvent @event)
        {
            return this.inner.PublishAsync(@event, this.request);
        }

        /// <summary>
        /// Asks the the processor to supply a service.
        /// The service will be created by the <see cref="IDependencyResolver"/>.
        /// If the ServiceProxyCreationEnabled is <c>true</c>, the service will be a proxy.
        /// </summary>
        /// <typeparam name="TService">The type of the service to supply.</typeparam>
        /// <returns>The service.</returns>
        public TService Using<TService>() where TService : class
        {
            return this.inner.Using<TService>();
        }
    }
}