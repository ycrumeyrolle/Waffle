namespace Waffle
{
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
        public static Task ProcessAsync(this IMessageProcessor processor, ICommand command)
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            return processor.ProcessAsync<VoidResult>(command);
        }

        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <param name="processor">The message processor.</param>
        /// <param name="command">The command to process.</param>
        /// <returns>The result of the command.</returns>
        public static void Process(this IMessageProcessor processor, ICommand command)
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            Task task = processor.ProcessAsync(command);
            task.Wait();
        }

        /// <summary>
        /// Process the command. 
        /// </summary>
        /// <param name="processor">The message processor.</param>
        /// <param name="command">The command to process.</param>
        /// <returns>The result of the command.</returns>
        public static TResult Process<TResult>(this IMessageProcessor processor, ICommand command)
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            Task<TResult> task = processor.ProcessAsync<TResult>(command);
            return task.Result;
        }
        
        /// <summary>
        /// Publish the event. 
        /// </summary>
        /// <param name="processor">The message processor.</param>
        /// <param name="event">The event to publish.</param>
        /// <returns>The result of the command.</returns>
        public static void Publish(this IMessageProcessor processor, IEvent @event)
        {
            if (processor == null)
            {
                throw Error.ArgumentNull("processor");
            }

            Task task = processor.PublishAsync(@event);
            task.Wait();
        }
    }
}
