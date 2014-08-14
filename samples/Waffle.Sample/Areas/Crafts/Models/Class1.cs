namespace Waffle.Sample.Areas.Crafts.Models
{
    using System;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Events;

    public class CutWood : ICommand
    {
        public int WoodId { get; set; }
    }

    public class WoodCutted : IEvent
    {
        public WoodCutted(Guid sourceId, int woodId)
        {
            this.SourceId = sourceId;
            this.WoodId = woodId;
        }

        public Guid SourceId
        {
            get;
            private set;
        }

        public int WoodId
        {
            get;
            private set;
        }
    }

    public class CutWoodHandler : MessageHandler, IAsyncCommandHandler<CutWood>
    {
        public Task HandleAsync(CutWood command)
        {
            WoodCutted @event = new WoodCutted(Guid.NewGuid(), command.WoodId);
            return this.CommandContext.Request.Processor.PublishAsync(@event);
        }
    }
}