namespace CommandProcessing.Eventing
{
    using System;

    /// <summary>
    /// Defines the methods that are required to handle event messaging.
    /// </summary>
    public interface IMessageHub
    {
        /// <summary>
        /// Subscribes for an event specified by <paramref name="eventName"/>.
        /// </summary>
        /// <param name="eventName">The name of the event to subscribe.</param>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="callback">THe action callback that will be called when the event will be triggered.</param>
        void Subscribe(string eventName, object subscriber, Action<object> callback);

        /// <summary>
        /// Unsubscribes for an event specified by <paramref name="eventName"/>.
        /// </summary>
        /// <param name="eventName">The name of the event to unsubscribe.</param>
        /// <param name="subscriber">The subscriber.</param>
        void Unsubscribe(string eventName, object subscriber);

        /// <summary>
        /// Unsubscribes for all events by a given <paramref name="subscriber"/>.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        void UnsubscribeAll(object subscriber);

        /// <summary>
        /// Trigger an event specified by <paramref name="eventName"/>.
        /// </summary>
        /// <param name="eventName">The name of the triggerered event.</param>
        /// <param name="context">The context of the event. This object will be supplied to the subscription callback.</param>
        void Publish(string eventName, object context);
    }
}