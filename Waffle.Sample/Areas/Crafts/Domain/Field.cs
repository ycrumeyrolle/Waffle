namespace Waffle.Sample.Areas.Crafts.Domain
{
    using System;

    public class  Field
    {
        public Seed Seed { get; set; }

        public Cereal Harvest()
        {
            if (this.Seed == null)
            {
                throw new InvalidOperationException("Unable to harvest this field. The field has bot been sowed.");
            }

            var cereal = this.Seed.Cereal;
            this.Seed = null;
            return cereal;
        }
    }
}
