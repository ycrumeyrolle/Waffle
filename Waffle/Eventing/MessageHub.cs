namespace Waffle.Eventing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;

    using Waffle.Internal;
    using Waffle.Tasks;

    /// <summary>
    /// Provides a default implementation of the <see cref="IMessageHub"/>.
    /// </summary>
    public class MessageHub : IMessageHub
    {
        private readonly ConcurrentDictionary<string, List<Tuple<object, Action<object>>>> store = new ConcurrentDictionary<string, List<Tuple<object, Action<object>>>>();

        /// <summary>
        /// Subscribes for an event specified by <paramref name="eventName"/>.
        /// </summary>
        /// <param name="eventName">The name of the event to subscribe.</param>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="callback">THe action callback that will be called when the event will be triggered.</param>
        public void Subscribe(string eventName, object subscriber, Action<object> callback)
        {
            var queue = this.store.GetOrAdd(eventName, _ => new List<Tuple<object, Action<object>>>());
            queue.Add(Tuple.Create(subscriber, callback));
        }

        /// <summary>
        /// Unsubscribes for an event specified by <paramref name="eventName"/>.
        /// </summary>
        /// <param name="eventName">The name of the event to unsubscribe.</param>
        /// <param name="subscriber">The subscriber.</param>
        public void Unsubscribe(string eventName, object subscriber)
        {
            List<Tuple<object, Action<object>>> queue;
            if (this.store.TryGetValue(eventName, out queue))
            {
                queue.RemoveAll(item => item.Item1 == subscriber);
            }
        }

        /// <summary>
        /// Unsubscribes for all events by a given <paramref name="subscriber"/>.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        public void UnsubscribeAll(object subscriber)
        {
            foreach (var queue in this.store.Values)
            {
                queue.RemoveAll(item => item.Item1 == subscriber);
            }
        }

        /// <summary>
        /// Trigger an event specified by <paramref name="eventName"/>.
        /// </summary>
        /// <param name="eventName">The name of the triggerered event.</param>
        /// <param name="context">The context of the event. This object will be supplied to the subscription callback.</param>
        public void Publish(string eventName, object context)
        {
            List<Tuple<object, Action<object>>> queue;
            if (this.store.TryGetValue(eventName, out queue))
            {
                var actions = queue.AsArray();

                var tasks = actions.Select(item => ExecutePublishingAsync(item.Item2, context));
                TaskHelpers.Iterate(tasks);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        private static Task ExecutePublishingAsync(Action<object> action, object value)
        {
            try
            {
                action(value);
                return TaskHelpers.Completed();
            }
            catch (Exception e)
            {
                return TaskHelpers.FromError(e);
            }
        }
    }
}