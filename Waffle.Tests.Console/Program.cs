using ConsoleProfiling = MiniProfiler.Windows.ConsoleProfiling;

namespace Waffle.Console
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Waffle;
    using Waffle.MiniProfiler;
    using Waffle.Tracing;
    using StackExchange.Profiling;
    using StackExchange.Profiling.Helpers;
    using Waffle.Filters;
    using Waffle.Validation;

    public class Program
    {
        public static void Main(string[] args)
        {
            ProcessorConfiguration config = new ProcessorConfiguration();
            config.Services.Replace(typeof(ICommandValidator), new NullValidator());
            ////     DefaultTraceWriter traceWorker = config.EnableSystemDiagnosticsTracing();


            ////  PerformanceTracer traceWriter = new PerformanceTracer();

            ////config.Services.Replace(typeof(ITraceWriter), traceWriter);
            ////    traceWorker.MinimumLevel = Tracing.TraceLevel.DefaultCommandValidator;
            config.Filters.Add(new CustomExceptionFilterAttribute());

         ////   long initialMemory = GC.GetTotalMemory(false);
            using (CommandProcessor processor = new CommandProcessor(config))
            {
                Command1 command1 = new Command1();
                Command2 command2 = new Command2();
                Command3 command3 = new Command3();
                Command4 command4 = new Command4();
                Command5 command5 = new Command5();
                Command6 command6 = new Command6();
                Command7 command7 = new Command7();
                Command8 command8 = new Command8();
                Command9 command9 = new Command9();
                Command10 command10 = new Command10();
                MultipleCommand1 command11 = new MultipleCommand1();
                MultipleCommand2 command12 = new MultipleCommand2();
                Parallel.For(0, 5000000, (i) =>
                    {
                        processor.Process(command1);
                        processor.Process(command2);
                        processor.Process(command3);
                        processor.Process(command4);
                        processor.Process(command5);
                        processor.Process(command6);
                        processor.Process(command7);
                        processor.Process(command8);
                        processor.Process(command9);
                        processor.Process(command10);
                        processor.Process<string>(command11);
                        processor.Process<string>(command12);
                    });

                ////for (int i = 0; i < 100000; i++)
                ////{
                ////    processor.Process(command1);
                ////    processor.Process(command2);
                ////    processor.Process(command3);
                ////    processor.Process(command4);
                ////    processor.Process(command5);
                ////    processor.Process(command6);
                ////    processor.Process(command7);
                ////    processor.Process(command8);
                ////    processor.Process(command9);
                ////    processor.Process(command10);
                ////    processor.Process<MultipleCommand1, string>(command11);
                ////    processor.Process<MultipleCommand2, string>(command12);
                ////}
            }
        }
    }

    public class MultipleCommand1 : Command
    {
    }

    public class MultipleCommand2 : Command
    {
    }

    public class MultipleHandler : Handler, IHandler<MultipleCommand1, string>, IHandler<MultipleCommand2, string>
    {
        public string Handle(MultipleCommand1 command)
        {
            return string.Empty;
        }

        public string Handle(MultipleCommand2 command)
        {
            return string.Empty;
        }
    }

    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HandlerExecutedContext handlerExecutedContext)
        {
            handlerExecutedContext.Result = VoidResult.Instance;
        }
    }

    public class NullValidator : ICommandValidator
    {
        public bool Validate(HandlerRequest request)
        {
            return true;
        }
    }

    public class Command1 : Command
    {
        [StringLength(10)]
        public string Value { get; set; }

        [Range(0, 50)]
        public string ValueInt { get; set; }

        [Range(0, 50)]
        public string ValueInt1 { get; set; }

        [Range(0, 50)]
        public string ValueInt2 { get; set; }

        [Range(0, 50)]
        public string ValueInt3 { get; set; }

        [Range(0, 50)]
        public string ValueInt4 { get; set; }
    }


    public class Handler1 : Handler<Command1>
    {
        public override void Handle(Command1 command)
        {
            // Nothing
        }
    }

    public class Command2 : Command
    {
        [StringLength(10)]
        public string Value { get; set; }

        [Range(0, 50)]
        public string ValueInt { get; set; }

        [Range(0, 50)]
        public string ValueInt1 { get; set; }

        [Range(0, 50)]
        public string ValueInt2 { get; set; }

        [Range(0, 50)]
        public string ValueInt3 { get; set; }

        [Range(0, 50)]
        public string ValueInt4 { get; set; }
    }


    public class Handler2 : Handler<Command2>
    {
        public override void Handle(Command2 command)
        {
            //  throw new Exception();
        }
    }

    public class Command3 : Command
    {
        [StringLength(10)]
        public string Value { get; set; }

        [Range(0, 50)]
        public string ValueInt { get; set; }

        [Range(0, 50)]
        public string ValueInt1 { get; set; }

        [Range(0, 50)]
        public string ValueInt2 { get; set; }

        [Range(0, 50)]
        public string ValueInt3 { get; set; }

        [Range(0, 50)]
        public string ValueInt4 { get; set; }
    }


    public class Handler3 : Handler<Command3>
    {
        public override void Handle(Command3 command)
        {
            // Nothing
        }
    }

    public class Command4 : Command
    {
        [StringLength(10)]
        public string Value { get; set; }

        [Range(0, 50)]
        public string ValueInt { get; set; }

        [Range(0, 50)]
        public string ValueInt1 { get; set; }

        [Range(0, 50)]
        public string ValueInt2 { get; set; }

        [Range(0, 50)]
        public string ValueInt3 { get; set; }

        [Range(0, 50)]
        public string ValueInt4 { get; set; }
    }


    public class Handler4 : Handler<Command4>
    {
        public override void Handle(Command4 command)
        {
            // Nothing
        }
    }

    public class Command5 : Command
    {
        [StringLength(10)]
        public string Value { get; set; }

        [Range(0, 50)]
        public string ValueInt { get; set; }

        [Range(0, 50)]
        public string ValueInt1 { get; set; }

        [Range(0, 50)]
        public string ValueInt2 { get; set; }

        [Range(0, 50)]
        public string ValueInt3 { get; set; }

        [Range(0, 50)]
        public string ValueInt4 { get; set; }
    }


    public class Handler5 : Handler<Command5>
    {
        public override void Handle(Command5 command)
        {
            // Nothing
        }
    }

    public class Command6 : Command
    {
        [StringLength(10)]
        public string Value { get; set; }

        [Range(0, 50)]
        public string ValueInt { get; set; }

        [Range(0, 50)]
        public string ValueInt1 { get; set; }

        [Range(0, 50)]
        public string ValueInt2 { get; set; }

        [Range(0, 50)]
        public string ValueInt3 { get; set; }

        [Range(0, 50)]
        public string ValueInt4 { get; set; }
    }


    public class Handler6 : Handler<Command6>
    {
        public override void Handle(Command6 command)
        {
            // Nothing
        }
    }

    public class Command7 : Command
    {
        [StringLength(10)]
        public string Value { get; set; }

        [Range(0, 50)]
        public string ValueInt { get; set; }

        [Range(0, 50)]
        public string ValueInt1 { get; set; }

        [Range(0, 50)]
        public string ValueInt2 { get; set; }

        [Range(0, 50)]
        public string ValueInt3 { get; set; }

        [Range(0, 50)]
        public string ValueInt4 { get; set; }
    }


    public class Handler7 : Handler<Command7>
    {
        public override void Handle(Command7 command)
        {
            // Nothing
        }
    }

    public class Command8 : Command
    {
        [StringLength(10)]
        public string Value { get; set; }

        [Range(0, 50)]
        public string ValueInt { get; set; }

        [Range(0, 50)]
        public string ValueInt1 { get; set; }

        [Range(0, 50)]
        public string ValueInt2 { get; set; }

        [Range(0, 50)]
        public string ValueInt3 { get; set; }

        [Range(0, 50)]
        public string ValueInt4 { get; set; }
    }


    public class Handler8 : Handler<Command8>
    {
        public override void Handle(Command8 command)
        {
            // Nothing
        }
    }

    public class Command9 : Command
    {
        [StringLength(10)]
        public string Value { get; set; }

        [Range(0, 50)]
        public string ValueInt { get; set; }

        [Range(0, 50)]
        public string ValueInt1 { get; set; }

        [Range(0, 50)]
        public string ValueInt2 { get; set; }

        [Range(0, 50)]
        public string ValueInt3 { get; set; }

        [Range(0, 50)]
        public string ValueInt4 { get; set; }
    }


    public class Handler9 : Handler<Command9>
    {
        public override void Handle(Command9 command)
        {
            // Nothing
        }
    }

    public class Command10 : Command
    {
        [StringLength(10)]
        public string Value { get; set; }

        [Range(0, 50)]
        public string ValueInt { get; set; }

        [Range(0, 50)]
        public string ValueInt1 { get; set; }

        [Range(0, 50)]
        public string ValueInt2 { get; set; }

        [Range(0, 50)]
        public string ValueInt3 { get; set; }

        [Range(0, 50)]
        public string ValueInt4 { get; set; }
    }

    public class Handler10 : Handler<Command10>
    {
        public override void Handle(Command10 command)
        {
            // Nothing
        }
    }
}