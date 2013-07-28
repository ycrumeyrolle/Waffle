namespace Waffle.Events.RavenDb.Tests
{
    using System;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Waffle.Events.RavenDB;

    [Ignore]
    [TestClass]
    public class RavenEventStoreFixture
    {
        [TestMethod]
        public void TestMethod1()
        {
            RavenEventStore eventStore = new RavenEventStore("http://localhost:8080/");

            EventTest1 @event1 = new EventTest1 { Test1 = "test", SourceId = Guid.NewGuid() };
            eventStore.StoreAsync(@event1, "eventTest", default(CancellationToken)).Wait();

            EventTest2 @event2 = new EventTest2 { Test2 = "test", SourceId = Guid.NewGuid() };
            eventStore.StoreAsync(@event2, "eventTest", default(CancellationToken)).Wait();

            var task1 = eventStore.LoadAsync(@event1.SourceId, default(CancellationToken));
            var result1 = task1.Result;
            Assert.IsNotNull(result1);

            var task2 = eventStore.LoadAsync(@event1.SourceId, default(CancellationToken));
            var result2 = task2.Result;
            Assert.IsNotNull(result2);
        }

        [TestMethod]
        public void TestMethod2()
        {
            RavenEventStore eventStore = new RavenEventStore("http://localhost:8080/");
            EventTest1 @event1 = new EventTest1 { Test1 = "test", SourceId = Guid.NewGuid() };
            eventStore.StoreAsync(@event1, "eventTest", default(CancellationToken)).Wait();

            var task = eventStore.LoadAsync(@event1.SourceId, default(CancellationToken));
            var result = task.Result;
        }

        public class EventTest1 : IEvent
        {
            /// <summary>
            /// Gets the identifier of the source originating the event.
            /// </summary>
            /// <value>The identifier of the source originating the event.</value>
            public Guid SourceId { get; set; }

            public string Test1 { get; set; }
        }

        public class EventTest2 : IEvent
        {
            /// <summary>
            /// Gets the identifier of the source originating the event.
            /// </summary>
            /// <value>The identifier of the source originating the event.</value>
            public Guid SourceId { get; set; }

            public string Test2 { get; set; }
        }
    }
}