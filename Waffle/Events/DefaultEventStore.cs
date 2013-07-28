namespace Waffle.Events
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Tasks;

    public class DefaultEventStore : IEventStore
    {
        private readonly ConcurrentQueue<IEvent> queue = new ConcurrentQueue<IEvent>();

        public Task StoreAsync(IEvent @event, string eventName, CancellationToken cancellationToken)
        {
            this.queue.Enqueue(@event);
            return TaskHelpers.Completed();
        }

        public Task<ICollection<IEvent>> LoadAsync(Guid sourceId, CancellationToken cancellationToken)
        {
            IEnumerable<IEvent> value = this.queue.Where(e => e.SourceId == sourceId);
            return TaskHelpers.FromResult<ICollection<IEvent>>(new ReadOnlyCollection<IEvent>(value.ToList()));
        }
    }
}