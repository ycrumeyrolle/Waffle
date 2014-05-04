﻿namespace Waffle.Tests.Integration
{
    using Microsoft.Practices.Unity;
    using Xunit;
    using Moq;
    using Waffle.Tests.Integration.Orders;
    using System.Threading.Tasks;
    using System.Threading;

    
    public class ProcessingCommandFixture
    {
        [Fact]
        public async Task PlaceOrder_EventsArePublished()
        {
            IUnityContainer container = new UnityContainer();

            Mock<ISpy> spy = new Mock<ISpy>();
            container.RegisterInstance(typeof(ISpy), spy.Object);

            using (ProcessorConfiguration config = new ProcessorConfiguration())
            {
                config.RegisterContainer(container);
                using (MessageProcessor processor = new MessageProcessor(config))
                {
                    PlaceOrder command = new PlaceOrder(1);
                    await processor.ProcessAsync(command);

                    // commands assertions
                    spy.Verify(s => s.Spy("PlaceOrder"), Times.Once());
                    spy.Verify(s => s.Spy("MakePayment"), Times.Once());
                    spy.Verify(s => s.Spy("MakeReservation"), Times.Once());
                    spy.Verify(s => s.Spy("AddSeatsToWaitList"), Times.Never());

                    // events assertions
                    spy.Verify(s => s.Spy("OrderConfirmed"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("OrderCreated"), Times.Once());
                    spy.Verify(s => s.Spy("PaymentAccepted"), Times.Once());
                    spy.Verify(s => s.Spy("SeatsReserved"), Times.Once());
                    spy.Verify(s => s.Spy("SeatsNotReserved"), Times.Never());

                    // ctor assertions
                    spy.Verify(s => s.Spy("OrderProcessManager ctor"), Times.Exactly(3));
                    spy.Verify(s => s.Spy("Order ctor"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("Payment ctor"), Times.Once());
                    spy.Verify(s => s.Spy("Payment ctor"), Times.Once());
                    spy.Verify(s => s.Spy("Payment ctor"), Times.Once());
                    spy.Verify(s => s.Spy("Reservation ctor"), Times.Exactly(2));
               }
            }
        }


        [Fact]
        public async Task PlaceOrder_WithMessageQueuing_EventsArePublished()
        {
            IUnityContainer container = new UnityContainer();

            Mock<ISpy> spy = new Mock<ISpy>();
            container.RegisterInstance(typeof(ISpy), spy.Object);

            using (ProcessorConfiguration config = new ProcessorConfiguration())
            {
                config.EnableInMemoryMessageQueuing();
                config.RegisterContainer(container);
                using (MessageProcessor processor = new MessageProcessor(config))
                {
                    PlaceOrder command = new PlaceOrder(1);
                    await processor.ProcessAsync(command);

                    CancellationTokenSource cts = new CancellationTokenSource(5);
                    await config.CommandBroker.RunAsync(cts.Token);

                    // commands assertions
                    spy.Verify(s => s.Spy("PlaceOrder"), Times.Once());
                    spy.Verify(s => s.Spy("MakePayment"), Times.Once());
                    spy.Verify(s => s.Spy("MakeReservation"), Times.Once());
                    spy.Verify(s => s.Spy("AddSeatsToWaitList"), Times.Never());

                    // events assertions
                    spy.Verify(s => s.Spy("OrderConfirmed"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("OrderCreated"), Times.Once());
                    spy.Verify(s => s.Spy("PaymentAccepted"), Times.Once());
                    spy.Verify(s => s.Spy("SeatsReserved"), Times.Once());
                    spy.Verify(s => s.Spy("SeatsNotReserved"), Times.Never());

                    // ctor assertions
                    spy.Verify(s => s.Spy("OrderProcessManager ctor"), Times.Exactly(3));
                    spy.Verify(s => s.Spy("Order ctor"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("Payment ctor"), Times.Once());
                    spy.Verify(s => s.Spy("Payment ctor"), Times.Once());
                    spy.Verify(s => s.Spy("Payment ctor"), Times.Once());
                    spy.Verify(s => s.Spy("Reservation ctor"), Times.Exactly(2));
                }
            }
        }
    }
}