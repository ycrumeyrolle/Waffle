namespace Waffle.Tests.Filters
{
    using System.Collections;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;

    [TestClass]
    public class HandlerFilterCollectionFixture
    {
        private readonly IFilter filter = new Mock<IFilter>().Object;
        private readonly HandlerFilterCollection collection = new HandlerFilterCollection();

        [TestMethod]
        public void WhenAddingItemWithFilterParameterIsNullThenThrowsArgumentNullException()
        {
            ExceptionAssert.ThrowsArgumentNull(() => this.collection.Add(filter: null), "filter");
        }

        [TestMethod]
        public void WhenAddingItemThenAddsFilterWithGlobalScope()
        {
            this.collection.Add(this.filter);

            Assert.AreSame(this.filter, this.collection.First().Instance);
            Assert.AreEqual(FilterScope.Global, this.collection.First().Scope);
        }

        [TestMethod]
        public void WhenAddingItemThenAllowsAddingSameInstanceMultipleTimes()
        {
            this.collection.Add(this.filter);
            this.collection.Add(this.filter);

            Assert.AreEqual(2, this.collection.Count);
        }

        [TestMethod]
        public void WhenClearingThenEmptiesCollection()
        {
            this.collection.Add(this.filter);

            this.collection.Clear();

            Assert.AreEqual(0, this.collection.Count);
        }

        [TestMethod]
        public void WhenCheckingContainsWithFilterNotInCollectionThenReturnsFalse()
        {
            Assert.IsFalse(this.collection.Contains(this.filter));
        }

        [TestMethod]
        public void WhenCheckingContainsWithFilterInCollectionThenReturnsTrue()
        {
            this.collection.Add(this.filter);

            Assert.IsTrue(this.collection.Contains(this.filter));
        }

        [TestMethod]
        public void WhenCountingWithEmptyCollectionThenReturnsZero()
        {
            Assert.AreEqual(0, this.collection.Count);
        }

        [TestMethod]
        public void WhenCountingWithOneItemAddedToCollectionThenReturnsOne()
        {
            this.collection.Add(this.filter);

            Assert.AreEqual(1, this.collection.Count);
        }

        [TestMethod]
        public void WhenRemovingWithEmptyCollectionThenDoesNothing()
        {
            this.collection.Remove(this.filter);

            Assert.AreEqual(0, this.collection.Count);
        }

        [TestMethod]
        public void WhenRemovingThenRemovesIt()
        {
            this.collection.Add(this.filter);
            this.collection.Add(this.filter);

            this.collection.Remove(this.filter);

            Assert.AreEqual(0, this.collection.Count);
        }

        [TestMethod]
        public void WhenEnumeratingThenIterates()
        {
            int counter = 0;
            this.collection.Add(this.filter);
            this.collection.Add(this.filter);

            foreach (var item in (IEnumerable)this.collection)
            {
                counter++;
            }

            Assert.AreEqual(2, counter);
        }

        [TestMethod]
        public void WhenCreatingFilterInfoWithNullParameterThenThrowsException()
        {   
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FilterInfo(null, FilterScope.Global), "instance");
        }
    }
}