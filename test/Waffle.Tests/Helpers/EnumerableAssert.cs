namespace Waffle.Tests.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public static class EnumerableAssert
    {
        public static void AreEqual(IEnumerable<object> listAsArray, IEnumerable<object> listToArray)
        {
            Assert.Equal(listToArray.Count(), listAsArray.Count());
            listAsArray.Zip(listToArray, (x, y) => { Assert.Equal(x, y); return true; });
        }
    }
}
