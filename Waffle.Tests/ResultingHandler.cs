namespace Waffle.Tests
{
    using System.Diagnostics;
    using Waffle;

    public class ResultingHandler : Handler<ResultingCommand, string>
    {
        public override string Handle(ResultingCommand command)
        {
            Trace.WriteLine("Property1 : " + command.Property1);

            Trace.WriteLine("Property2 : " + command.Property2);

            return command.Property1 + " - " + command.Property2;
        }
    }
}