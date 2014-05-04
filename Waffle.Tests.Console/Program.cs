namespace Waffle.Tests.Console
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Practices.Unity;
    using Moq;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Tests.Integration;
    using Waffle.Tests.Integration.Orders;
    using Waffle.Validation;
    using System.Diagnostics;
    using Waffle.Filters;
    using Waffle.Queuing;
    using System.Threading;

    public static class Program
    {
        private class TestCommand : ICommand
        {

        }
        private class TestCommand2 : ICommand
        {

        }


        public static void Main()
        {
            using (IUnityContainer container = new UnityContainer())
            {
                container.RegisterInstance(typeof(ISpy), new NullSpy());

                using (ProcessorConfiguration config = new ProcessorConfiguration())
                {
                    config.DefaultHandlerLifetime = HandlerLifetime.Transient;

                    config.RegisterCommandHandler<TestCommand>(async (command) =>
                    {
                        await Task.FromResult(0);
                    });

                    config.RegisterCommandHandler<TestCommand2, string>(async command =>
                    {
                        return await Task.FromResult("test");
                    });

                    //  config.RegisterContainer(container);
                    // config.Services.Replace(typeof(ICommandValidator), new NullValidator());
                    //  config.EnableDefaultTracing();

                    ////  PerformanceTracer traceWriter = new PerformanceTracer();

                    ////config.Services.Replace(typeof(ITraceWriter), traceWriter);
                    ////    traceWorker.MinimumLevel = Tracing.TraceLevel.DefaultCommandValidator;
                    ////config.Filters.Add(new CustomExceptionFilterAttribute());

                    ////   long initialMemory = GC.GetTotalMemory(false);

                    //    config.EnableGlobalExceptionHandler();

                    const int maxIterations = 1;
                    config.EnableInMemoryMessageQueuing();
                    using (MessageProcessor processor = new MessageProcessor(config))
                    {
                        processor.ProcessAsync(new TestCommand());
                        SingleProcessing(processor);
                        ParallelProcessing(maxIterations, processor);
                        //   SequentialTaskProcessing(maxIterations, processor);
                        //   SequentialTaskProcessingV2(maxIterations, processor);
                        //  SequentialTaskProcessingV3(maxIterations, processor);

                        RunCommandBroker(config.CommandBroker);
                    }

                }

            //    Console.ReadLine();
            }
        }

        private static void RunCommandBroker(CommandBroker broker)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var task = broker.RunAsync(cts.Token);
            Console.WriteLine("Press any key to stop...");
            Console.Read();
            cts.Cancel();
        }

        private static async void SingleProcessing(MessageProcessor processor)
        {
            PlaceOrder command = new PlaceOrder(10);
            await processor.ProcessAsync(command);
        }

        private static void ParallelProcessing(int maxIterations, MessageProcessor processor)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Parallel.For(0, maxIterations, async (i) =>
            {
                PlaceOrder command = new PlaceOrder(1);
                await processor.ProcessAsync(command);
            });
            stopwatch.Stop();
            System.Console.WriteLine("Parallel for, " + maxIterations + " iterations : " + stopwatch.ElapsedMilliseconds + " ms");
        }

        private static void SequentialTaskProcessing(int maxIterations, MessageProcessor processor)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Task[] tasks = new Task[maxIterations];
            for (int i = 0; i < maxIterations; i++)
            {
                PlaceOrder command = new PlaceOrder(1);
                tasks[i] = processor.ProcessAsync(command);
            }

            Task.WaitAll(tasks);
            stopwatch.Stop();
            System.Console.WriteLine("Sequential Tasks, " + maxIterations + " iterations : " + stopwatch.ElapsedMilliseconds + " ms");
        }

        private static async void SequentialTaskProcessingV2(int maxIterations, MessageProcessor processor)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < maxIterations; i++)
            {
                PlaceOrder command = new PlaceOrder(1);
                await processor.ProcessAsync(command);
            }

            stopwatch.Stop();
            System.Console.WriteLine("Sequential Tasks v2, " + maxIterations + " iterations : " + stopwatch.ElapsedMilliseconds + " ms");
        }

        private static async void SequentialTaskProcessingV3(int maxIterations, MessageProcessor processor)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            TaskCompletionSource<HandlerResponse> tcs = new TaskCompletionSource<HandlerResponse>();
            tcs.SetResult(null);
            Task task = tcs.Task;
            for (int i = 0; i < maxIterations; i++)
            {
                PlaceOrder command = new PlaceOrder(1);
                task = task.ContinueWith(t => processor.ProcessAsync(command));
            }

            await task;
            stopwatch.Stop();
            System.Console.WriteLine("Sequential Tasks v3, " + maxIterations + " iterations : " + stopwatch.ElapsedMilliseconds + " ms");
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