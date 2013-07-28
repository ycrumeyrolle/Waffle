namespace Waffle.Tests
{
    using System.Diagnostics;
    using Waffle.Caching;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Tests.Commands;

    public class SimpleCommandHandler : CommandHandler<SimpleCommand>
    {
        public override void Handle(SimpleCommand command, CommandHandlerContext context)
        {
            Trace.WriteLine("Property1 : " + command.Property1);

            Trace.WriteLine("Property2 : " + command.Property2);
        }
    }

    [NoCache]
    public class NotCachedCommandHandler : CommandHandler<NotCachedCommand>
    {
        public override void Handle(NotCachedCommand command, CommandHandlerContext context)
        {
            Trace.WriteLine("Property1 : " + command.Property1);

            Trace.WriteLine("Property2 : " + command.Property2);
        }
    }
}