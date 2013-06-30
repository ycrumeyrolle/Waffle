namespace Waffle.Tests
{
    using System;
    using System.Diagnostics;
    using Waffle;
    using Waffle.Caching;

    public class SimpleHandler : Handler<SimpleCommand>
    {
        public override void Handle(SimpleCommand command)
        {
            Trace.WriteLine("Property1 : " + command.Property1);

            Trace.WriteLine("Property2 : " + command.Property2);
        }
    }

    [NoCache]
    public class NotCachedHandler : Handler<NotCachedCommand>
    {
        public override void Handle(NotCachedCommand command)
        {
            Trace.WriteLine("Property1 : " + command.Property1);

            Trace.WriteLine("Property2 : " + command.Property2);
        }
    }
}