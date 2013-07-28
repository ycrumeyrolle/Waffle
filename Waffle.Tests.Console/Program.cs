namespace Waffle.Tests.Console
{
    using System.Threading.Tasks;
    using Microsoft.Practices.Unity;
    using Moq;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Tests.Integration;
    using Waffle.Tests.Integration.Orders;
    using Waffle.Validation;

    public static class Program
    {
        public static void Main()
        {
            IUnityContainer container = new UnityContainer();
          
            container.RegisterInstance(typeof(ISpy), new NullSpy());

            using (ProcessorConfiguration config = new ProcessorConfiguration())
            {
                config.RegisterContainer(container);
                // config.Services.Replace(typeof(ICommandValidator), new NullValidator());
                ////DefaultTraceWriter traceWorker = config.EnableDefaultTracing();


                ////  PerformanceTracer traceWriter = new PerformanceTracer();

                ////config.Services.Replace(typeof(ITraceWriter), traceWriter);
                ////    traceWorker.MinimumLevel = Tracing.TraceLevel.DefaultCommandValidator;
                ////config.Filters.Add(new CustomExceptionFilterAttribute());

                ////   long initialMemory = GC.GetTotalMemory(false);
                using (MessageProcessor processor = new MessageProcessor(config))
                {
                    Parallel.For(0, 1000000, i =>
                        {
                            PlaceOrder command = new PlaceOrder();
                            processor.Process(command);
                        });

                    //for (int i = 0; i < 100000; i++)
                    //{
                    //    PlaceOrder command = new PlaceOrder();
                    //    processor.Process(command);
                    //}
                }
            }
        }

        private class NullValidator : ICommandValidator
        {
            /// <summary>
            /// Determines whether the command is valid and adds any validation errors to the command's ValidationResults.
            /// </summary>
            /// <param name="request">The <see cref="HandlerRequest"/> to be validated.</param>
            /// <returns>true if command is valid, false otherwise.</returns>
            public bool Validate(CommandHandlerRequest request)
            {
                return true;
            }
        }

        private class NullSpy : ISpy
        {
            public void Spy(string name)
            {
            }
        }
    }
}