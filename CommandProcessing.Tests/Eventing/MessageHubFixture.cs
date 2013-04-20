namespace CommandProcessing.Tests.Eventing
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandProcessing.Eventing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class MessageHubFixture
    {
        [TestMethod]
        public void WhenPublishingEventThenCallbackAreCalled()
        {
            // Arrange
            Mock<ICallback> callback1 = new Mock<ICallback>();
            callback1.Setup(c => c.Callback(It.IsAny<object>()));
            Mock<ICallback> callback2 = new Mock<ICallback>();
            callback2.Setup(c => c.Callback(It.IsAny<object>()));
            Mock<ICallback> callback3 = new Mock<ICallback>();
            callback3.Setup(c => c.Callback(It.IsAny<object>()));
            MessageHub hub = new MessageHub();
            hub.Subscribe("event1", this, callback1.Object.Callback);
            hub.Subscribe("event2", this, callback2.Object.Callback);
            hub.Subscribe("event3", this, callback3.Object.Callback);
            
            SimpleCommand command = new SimpleCommand();
            hub.Publish("event1", command);

            // Assert
            callback1.Verify(c => c.Callback(It.IsAny<object>()), Times.Exactly(1));
            callback2.Verify(c => c.Callback(It.IsAny<object>()), Times.Exactly(0));
            callback3.Verify(c => c.Callback(It.IsAny<object>()), Times.Exactly(0));
        }

        [TestMethod]
        public void WhenPublishingEventThrowExceptionThenCallbackChainingIsStopped()
        {
            // Arrange
            Mock<ICallback> callback1 = new Mock<ICallback>();
            callback1.Setup(c => c.Callback(It.IsAny<object>())).Throws<Exception>();
            Mock<ICallback> callback2 = new Mock<ICallback>();
            callback2.Setup(c => c.Callback(It.IsAny<object>()));
            Mock<ICallback> callback3 = new Mock<ICallback>();
            callback3.Setup(c => c.Callback(It.IsAny<object>()));
            MessageHub hub = new MessageHub();
            hub.Subscribe("event1", this, callback1.Object.Callback);
            hub.Subscribe("event1", this, callback1.Object.Callback);
            hub.Subscribe("event2", this, callback2.Object.Callback);
            hub.Subscribe("event3", this, callback3.Object.Callback);

            SimpleCommand command = new SimpleCommand();
            hub.Publish("event1", command);

            // Assert
            callback1.Verify(c => c.Callback(It.IsAny<object>()), Times.Exactly(1));
            callback2.Verify(c => c.Callback(It.IsAny<object>()), Times.Exactly(0));
            callback3.Verify(c => c.Callback(It.IsAny<object>()), Times.Exactly(0));
        }
        
        [TestMethod]
        public void WhenUnsubscribingEventThenCallbackAreNotCalled()
        {
            // Arrange
            Mock<ICallback> callback1 = new Mock<ICallback>();
            callback1.Setup(c => c.Callback(It.IsAny<object>()));
            MessageHub hub = new MessageHub();
            hub.Subscribe("event1", this, callback1.Object.Callback);
            hub.Unsubscribe("event1", this);

            SimpleCommand command = new SimpleCommand();
            hub.Publish("event1", command);

            // Assert
            callback1.Verify(c => c.Callback(It.IsAny<object>()), Times.Never());
        }
        
        [TestMethod]
        public void WhenUnsubscribingAllEventsThenCallbackAreNotCalled()
        {
            // Arrange
            Mock<ICallback> callback1 = new Mock<ICallback>();
            callback1.Setup(c => c.Callback(It.IsAny<object>()));
            MessageHub hub = new MessageHub();
            hub.Subscribe("event1", this, callback1.Object.Callback);
            hub.UnsubscribeAll(this);

            SimpleCommand command = new SimpleCommand();
            hub.Publish("event1", command);

            // Assert
            callback1.Verify(c => c.Callback(It.IsAny<object>()), Times.Never());
        }

        [TestMethod]
        public void WhenPublishingMassEventsThenCallbackAreCalled()
        {
            // Arrange
            Mock<ICallback> callback1 = new Mock<ICallback>();
            callback1.Setup(c => c.Callback(It.IsAny<object>())).Callback(this.Increment);
            Mock<ICallback> callback2 = new Mock<ICallback>();
            callback2.Setup(c => c.Callback(It.IsAny<object>()));
            Mock<ICallback> callback3 = new Mock<ICallback>();
            callback3.Setup(c => c.Callback(It.IsAny<object>()));
            MessageHub hub = new MessageHub();
            hub.Subscribe("event1", this, callback1.Object.Callback);
            hub.Subscribe("event2", this, callback2.Object.Callback);
            hub.Subscribe("event3", this, callback3.Object.Callback);

            var result = Parallel.For(0, 1000, i => hub.Publish("event1", i));

            while (!result.IsCompleted)
            {
                Thread.Sleep(10);
            }

            // Assert
            Assert.AreEqual(1000, this.value);

            // Does not work well on Parallel testing.
            // callback1.Verify(c => c.Callback(It.IsAny<object>()), Times.Exactly(1000));
            callback2.Verify(c => c.Callback(It.IsAny<object>()), Times.Exactly(0));
            callback3.Verify(c => c.Callback(It.IsAny<object>()), Times.Exactly(0));
        }

        private readonly object syncLock = new object();

        private int value = 0;

        private void Increment()
        {
            lock (this.syncLock)
            {
                this.value++;
            }
        }

        public interface ICallback
        {
            void Callback(object context);
        }
    }
}