namespace CommandProcessing.Tests
{
    using CommandProcessing;

    public class SimpleHandler : Handler<SimpleCommand, string>
    {
        public override string Handle(SimpleCommand command)
        {
            System.Diagnostics.Trace.WriteLine("Property1 : " + command.Property1);

            System.Diagnostics.Trace.WriteLine("Property2 : " + command.Property2);

            // throw new Exception();
            return command.Property1 + " - " + command.Property2;
        }
    }
}