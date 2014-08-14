namespace Waffle.Sample.Areas.Crafts.Domain
{
    public class Bread : IStoreable
    {
        public Bread(Flour flour)
        {
            this.Flour = flour;
        }

        public Flour Flour { get; private set; }
    }
}
