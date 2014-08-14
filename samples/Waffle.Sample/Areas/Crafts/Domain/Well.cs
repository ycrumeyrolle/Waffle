namespace Waffle.Sample.Areas.Crafts.Domain
{
    using System;
    using System.Threading;

    public class Well
    {
        private int capacity;

        public Well(int capacity)
        {
            this.capacity = capacity;
        }

        public Bucket Collect()
        {
            if (this.capacity <= 0)
            {
                throw new InvalidOperationException("The well is empty!");
            }

            Interlocked.Decrement(ref this.capacity);
            return new Bucket();
        }
    }
}
