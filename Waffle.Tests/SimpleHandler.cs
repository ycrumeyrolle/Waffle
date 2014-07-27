namespace Waffle.Tests
{
    using System.Diagnostics;
    using Waffle.Commands;

    public class SimpleCommandHandler : MessageHandler, ICommandHandler<SimpleCommand>
    {
        public void Handle(SimpleCommand command)
        {
            Trace.WriteLine("Property1 : " + command.Property1);

            Trace.WriteLine("Property2 : " + command.Property2);
        }
    }

    public class SimpleCommandHandler2 : MessageHandler, ICommandHandler<SimpleCommand2, string>
    {
        public string Handle(SimpleCommand2 command)
        {
            Trace.WriteLine("Property1 : " + command.Property1);

            Trace.WriteLine("Property2 : " + command.Property2);

            return "RESULT";
        }
    }
}