namespace Waffle.Tests
{
    using System.Diagnostics;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Tests.Commands;

    public class ResultingCommandHandler : CommandHandler<ResultingCommand, string>
    {
        public override string Handle(ResultingCommand command, CommandHandlerContext context)
        {
            Trace.WriteLine("Property1 : " + command.Property1);

            Trace.WriteLine("Property2 : " + command.Property2);

            return command.Property1 + " - " + command.Property2;
        }
    }
}