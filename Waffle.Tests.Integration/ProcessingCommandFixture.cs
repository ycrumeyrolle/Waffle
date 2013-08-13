﻿namespace Waffle.Tests.Integration
{
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Tests.Integration.Orders;

    [TestClass]
    public class ProcessingCommandFixture
    {
        [TestMethod]
        public void PlaceOrder_EventsArePublished()
        {
            IUnityContainer container = new UnityContainer();
          
            Mock<ISpy> spy = new Mock<ISpy>();
            container.RegisterInstance(typeof(ISpy), spy.Object);

            using (ProcessorConfiguration config = new ProcessorConfiguration())
            {
                config.RegisterContainer(container);
                using (MessageProcessor processor = new MessageProcessor(config))
                {
                    PlaceOrder command = new PlaceOrder();
                    processor.Process(command);
                    
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
                    spy.Verify(s => s.Spy("OrderProcessManager ctor"), Times.Once());
                    spy.Verify(s => s.Spy("Order ctor"), Times.Once());
                    spy.Verify(s => s.Spy("Payment ctor"), Times.Once());
                    spy.Verify(s => s.Spy("Payment ctor"), Times.Once());
                    spy.Verify(s => s.Spy("Payment ctor"), Times.Once());
                    spy.Verify(s => s.Spy("Reservation ctor"), Times.Once()); 
                    
                    
                    command = new PlaceOrder();
                    processor.Process(command);

                    // commands assertions
                    spy.Verify(s => s.Spy("PlaceOrder"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("MakePayment"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("MakeReservation"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("AddSeatsToWaitList"), Times.Never());

                    // events assertions
                    spy.Verify(s => s.Spy("OrderConfirmed"), Times.Exactly(4));
                    spy.Verify(s => s.Spy("OrderCreated"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("PaymentAccepted"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("SeatsReserved"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("SeatsNotReserved"), Times.Never());

                    // ctor assertions
                    spy.Verify(s => s.Spy("OrderProcessManager ctor"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("Order ctor"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("Payment ctor"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("Payment ctor"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("Payment ctor"), Times.Exactly(2));
                    spy.Verify(s => s.Spy("Reservation ctor"), Times.Exactly(2));
                }
            }
        }
    }
}