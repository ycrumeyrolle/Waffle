namespace Waffle
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Internal;

    /// <summary>
    /// Provides methods extensions to the <see cref="MessageProcessor"/>.
    /// </summary>
    public static class MessageProcessorExtensions
    {
        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <param name="processor">The message processor.</param>
        /// <param name="command">The command to process.</param>
        /// <returns>The <see cref="Task"/> returning the result of the command.</returns>
        public static Task<HandlerResponse> ProcessAsync(this IMessageProcessor processor, ICommand command)
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            return processor.ProcessAsync(command, default(CancellationToken));
        }

#if LOOSE_CQRS
        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <param name="processor">The message processor.</param>
        /// <param name="command">The command to process.</param>
        /// <returns>The <see cref="Task"/> returning the result of the command.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nesting is required to return Task.")]
        public static Task<HandlerResponse<TResult>> ProcessAsync<TResult>(this IMessageProcessor processor, ICommand command)
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            return processor.ProcessAsync<TResult>(command, default(CancellationToken));
        }
#endif

#if LOOSE_CQRS
        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <param name="processor">The message processor.</param>
        /// <param name="command">The command to process.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="Task"/> returning the result of the command.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nesting is required to return Task.")]
        public static async Task<HandlerResponse<TResult>> ProcessAsync<TResult>(this IMessageProcessor processor, ICommand command, CancellationToken cancellationToken)
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            var response = await processor.ProcessAsync(command, cancellationToken);
            return new HandlerResponse<TResult>(response);
        }
#endif
        
        /// <summary>
        /// Publish the event. 
        /// </summary>
        /// <param name="processor">The message processor.</param>
        /// <param name="event">The event to publish.</param>
        /// <returns>The <see cref="Task"/> representing the event handling.</returns>
        public static Task PublishAsync(this IMessageProcessor processor, IEvent @event)
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            return processor.PublishAsync(@event, default(CancellationToken));
        }

        /////// <summary>
        /////// Publish the event. 
        /////// </summary>
        /////// <param name="processor">The message processor.</param>
        /////// <param name="event">The event to publish.</param>
        ////public static async void Publish(this IMessageProcessor processor, IEvent @event)
        ////{
        ////    if (processor == null)
        ////    {
        ////        throw Error.ArgumentNull("processor");
        ////    }

        ////    await processor.PublishAsync(@event, default(CancellationToken));
        ////}
    }
}
