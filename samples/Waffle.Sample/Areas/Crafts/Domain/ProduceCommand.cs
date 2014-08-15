namespace Waffle.Sample.Areas.Crafts.Domain
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Filters;

    public class CollectWater : ICommand
    {
        public CollectWater(Well well)
        {
            this.Well = well;
        }

        public Well Well { get; private set; }
    }

    public class WaterCollected : IStorageEvent
    {
        public WaterCollected(Well well, Bucket bucket)
        {
            this.Well = well;
            this.Bucket = bucket;
        }

        public Well Well { get; private set; }

        public Bucket Bucket { get; private set; }

        public IStoreable Object
        {
            get { return this.Bucket; }
        }
    }

    public class SowField : ICommand
    {
        public SowField(Field field, Seed seed)
        {
            this.Field = field;
            this.Seed = seed;
        }

        public Field Field { get; private set; }

        public Seed Seed { get; private set; }
    }

    public class FieldSowed : IEvent
    {
        public FieldSowed(Field field, Seed seed)
        {
            this.Field = field;
            this.Seed = seed;
        }

        public Field Field { get; private set; }

        public Seed Seed { get; private set; }
    }

    public class HarvestField : ICommand
    {
        public Field Field { get; private set; }
    }

    public class FieldHarvested : IStorageEvent
    {
        public FieldHarvested(Field field, Cereal cereal)
        {
            this.Field = field;
            this.Cereal = cereal;
        }

        public Field Field { get; private set; }

        public Cereal Cereal { get; private set; }

        public IStoreable Object
        {
            get
            {
                return this.Cereal;
            }
        }
    }

    public class GrindFlour : ICommand
    {
        public GrindFlour(Cereal cereal)
        {
            this.Cereal = cereal;
        }

        public Cereal Cereal { get; private set; }
    }

    public class FlourGrinded : IStorageEvent
    {
        public FlourGrinded(Cereal cereal, Flour flour)
        {
            this.Cereal = cereal;
            this.Flour = flour;
        }

        public Cereal Cereal { get; private set; }

        public Flour Flour { get; private set; }

        public IStoreable Object
        {
            get
            {
                return this.Flour;
            }
        }
    }

    public class BakeBread : ICommand
    {
        public BakeBread(Flour flour, Bucket bucket)
        {
            this.Flour = flour;
            this.Bucket = bucket;
        }

        public Flour Flour { get; private set; }

        public Bucket Bucket { get; private set; }
    }

    public class BreadBaked : IStorageEvent
    {
        public BreadBaked(Bread bread)
        {
            this.Bread = bread;
        }

        public Bread Bread { get; private set; }

        public IStoreable Object
        {
            get
            {
                return this.Bread;
            }
        }
    }

    public class Farmer : CommandHandler,
        IAsyncCommandHandler<SowField>,
        IAsyncCommandHandler<HarvestField>
    {
        /// <summary>
        /// Handle the <see cref="SowField"/> command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <returns>The result object.</returns>
        public async Task HandleAsync(SowField command)
        {
            await Task.Delay(1000);
            command.Field.Seed = command.Seed;

            var fieldSowed = new FieldSowed(command.Field, command.Seed);
            await this.CommandContext.PublishAsync(fieldSowed);
        }

        /// <summary>
        /// Handle the <see cref="HarvestField"/> command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <returns>The result object.</returns>
        public async Task HandleAsync(HarvestField command)
        {
            await Task.Delay(1000);
            Cereal cereal = command.Field.Harvest();

            var fieldHarvested = new FieldHarvested(command.Field, cereal);
            await this.CommandContext.PublishAsync(fieldHarvested);
        }
    }

    public class Baker : MessageHandler,
        IAsyncCommandHandler<BakeBread>,
        IAsyncEventHandler<WaterCollected>
    {
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <returns>The result object.</returns>
        public async Task HandleAsync(BakeBread command)
        {
            await Task.Delay(1000);
            Bread bread = new Bread(command.Flour);

            var breadBaked = new BreadBaked(bread);
            await this.CommandContext.PublishAsync(breadBaked);
        }

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        public Task HandleAsync(WaterCollected @event)
        {
            throw new NotImplementedException();
        }
    }

    public class WellDriller : CommandHandler,
        IAsyncCommandHandler<CollectWater>
    {
        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <returns>The result object.</returns>
        public async Task HandleAsync(CollectWater command)
        {
            await Task.Delay(1000);
            Bucket bucket = command.Well.Collect();

            var waterCollected = new WaterCollected(command.Well, bucket);
            await this.CommandContext.PublishAsync(waterCollected);
        }
    }

    public class Peon : MessageHandler,
        IAsyncEventHandler<IStorageEvent>
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentQueue<IStoreable>> Store = new ConcurrentDictionary<Type, ConcurrentQueue<IStoreable>>();

        private readonly IList<IRecipe> recipes = new List<IRecipe>
                                                  {
            new BreadRecipe()
        };

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        public Task HandleAsync(IStorageEvent @event)
        {
            ConcurrentQueue<IStoreable> queue;
            if (!Store.TryGetValue(@event.GetType(), out queue))
            {
                queue = new ConcurrentQueue<IStoreable>();
            }

            queue.Enqueue(@event.Object);

            return Task.WhenAll(this.CheckForAvailableRecipes());
        }

        private IEnumerable<Task> CheckForAvailableRecipes()
        {
            for (int i = 0; i < this.recipes.Count; i++)
            {
                var recipe = this.recipes[i];
                var command = recipe.PrepareCommand(Store);
                if (command != null)
                {
                    yield return this.EventContext.ProcessAsync(command);
                }
            }
        }
    }

    public interface IRecipe
    {
        ICommand PrepareCommand(ConcurrentDictionary<Type, ConcurrentQueue<IStoreable>> store);
    }

    public class BreadRecipe : IRecipe
    {
        public ICommand PrepareCommand(ConcurrentDictionary<Type, ConcurrentQueue<IStoreable>> store)
        {
            ConcurrentQueue<IStoreable> flourStock;

            if (!store.TryGetValue(typeof(Flour), out flourStock))
            {
                return null;
            }

            ConcurrentQueue<IStoreable> bucketStock;
            if (!store.TryGetValue(typeof(Bucket), out bucketStock))
            {
                return null;
            }

            IStoreable flour;
            if (!flourStock.TryDequeue(out flour))
            {
                return null;
            }

            IStoreable bucket;
            if (!bucketStock.TryDequeue(out bucket))
            {
                flourStock.Enqueue(flour);
                return null;
            }

            return new BakeBread((Flour)flour, (Bucket)bucket);
        }
    }

    public interface IStorageEvent : IEvent
    {
        IStoreable Object { get; }
    }

    public interface IStoreable
    {
    }

    public static class CommandHandlerContextExtensions
    {
        public static Task PublishAsync(this CommandHandlerContext context, IEvent @event)
        {
            var processor = context.Configuration.Services.GetProcessor();
            return processor.PublishAsync(@event);
        }

        public static Task ProcessAsync(this EventHandlerContext context, ICommand command)
        {
            var processor = context.Configuration.Services.GetProcessor();
            return processor.ProcessAsync(command);
        }
    }
}