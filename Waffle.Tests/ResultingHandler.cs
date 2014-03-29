namespace Waffle.Tests
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Tests.Commands;

    public class ResultingCommandHandler : MessageHandler, ICommandHandler<ResultingCommand, string>
    {
        public  string Handle(ResultingCommand command)
        {
            Trace.WriteLine("Property1 : " + command.Property1);

            Trace.WriteLine("Property2 : " + command.Property2);

            return command.Property1 + " - " + command.Property2;
        }
    }
}