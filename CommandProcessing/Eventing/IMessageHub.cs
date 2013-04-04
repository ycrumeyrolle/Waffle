namespace CommandProcessing.Eventing
{
    using System;

    public interface IMessageHub
    {
        void Subscribe(string eventName, object subscriber, Action<object> callback);

        void Unsubscribe(string eventName, object subscriber);

        void UnsubscribeAll(object subscriber);

        void Publish(string eventName, object context);
    }
}