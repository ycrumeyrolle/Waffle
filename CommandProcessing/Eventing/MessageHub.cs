namespace CommandProcessing.Eventing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using CommandProcessing.Internal;
    using CommandProcessing.Tasks;

    public class MessageHub : IMessageHub
    {
        private readonly ConcurrentDictionary<string, List<Tuple<object, Action<object>>>> store = new ConcurrentDictionary<string, List<Tuple<object, Action<object>>>>();
        
        public void Subscribe(string eventName, object subscriber, Action<object> callback)
        {
            var queue = this.store.GetOrAdd(eventName, _ => new List<Tuple<object, Action<object>>>());
            queue.Add(Tuple.Create(subscriber, callback));
        }

        public void Unsubscribe(string eventName, object subscriber)
        {
            List<Tuple<object, Action<object>>> queue;
            if (this.store.TryGetValue(eventName, out queue))
            {
                queue.RemoveAll(item => item.Item1 == subscriber);
            }
        }

        public void UnsubscribeAll(object subscriber)
        {
            foreach (var queue in this.store.Values)
            {
                queue.RemoveAll(item => item.Item1 == subscriber);
            }
        }

        public void Publish(string eventName, object context)
        {
            List<Tuple<object, Action<object>>> queue;
            if (this.store.TryGetValue(eventName, out queue))
            {
                var actions = queue.AsArray();
               
                var tasks = actions.Select(item => TaskHelpers.RunSynchronously(() => item.Item2(context)));
                TaskHelpers.Iterate(tasks);
            }
        }
    }
}