namespace CommandProcessing.Tests.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class EnumerableAssert
    {
        public static void AreEqual(IEnumerable<object> listAsArray, IEnumerable<object> listToArray)
        {
            Assert.AreEqual(listToArray.Count(), listAsArray.Count());
            listAsArray.Zip(listToArray, (x, y) => { Assert.AreEqual(x, y); return true; });
        }
    }
}
