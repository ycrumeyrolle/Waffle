namespace Waffle.Tests.Filters
{
    using System.Linq;
    using Moq;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;
    using Xunit;

    public class HandlerFilterCollectionTests
    {
        private readonly IFilter filter = new Mock<IFilter>().Object;
        private readonly HandlerFilterCollection collection = new HandlerFilterCollection();

        [Fact]
        public void WhenAddingItemWithFilterParameterIsNullThenThrowsArgumentNullException()
        {
            ExceptionAssert.ThrowsArgumentNull(() => this.collection.Add(filter: null), "filter");
        }

        [Fact]
        public void WhenAddingItemThenAddsFilterWithGlobalScope()
        {
            this.collection.Add(this.filter);

            Assert.Same(this.filter, this.collection.First().Instance);
            Assert.Equal(FilterScope.Global, this.collection.First().Scope);
        }

        [Fact]
        public void WhenAddingItemThenAllowsAddingSameInstanceMultipleTimes()
        {
            this.collection.Add(this.filter);
            this.collection.Add(this.filter);

            Assert.Equal(2, this.collection.Count);
        }

        [Fact]
        public void WhenClearingThenEmptiesCollection()
        {
            this.collection.Add(this.filter);

            this.collection.Clear();

            Assert.Equal(0, this.collection.Count);
        }

        [Fact]
        public void WhenCheckingContainsWithFilterNotInCollectionThenReturnsFalse()
        {
            Assert.False(this.collection.Contains(this.filter));
        }

        [Fact]
        public void WhenCheckingContainsWithFilterInCollectionThenReturnsTrue()
        {
            this.collection.Add(this.filter);

            Assert.True(this.collection.Contains(this.filter));
        }

        [Fact]
        public void WhenCountingWithEmptyCollectionThenReturnsZero()
        {
            Assert.Equal(0, this.collection.Count);
        }

        [Fact]
        public void WhenCountingWithOneItemAddedToCollectionThenReturnsOne()
        {
            this.collection.Add(this.filter);

            Assert.Equal(1, this.collection.Count);
        }

        [Fact]
        public void WhenRemovingWithEmptyCollectionThenDoesNothing()
        {
            this.collection.Remove(this.filter);

            Assert.Equal(0, this.collection.Count);
        }

        [Fact]
        public void WhenRemovingThenRemovesIt()
        {
            this.collection.Add(this.filter);
            this.collection.Add(this.filter);

            this.collection.Remove(this.filter);

            Assert.Equal(0, this.collection.Count);
        }

        [Fact]
        public void WhenEnumeratingThenIterates()
        {
            this.collection.Add(this.filter);
            this.collection.Add(this.filter);

            int counter = this.collection.Cast<object>().Count();

            Assert.Equal(2, counter);
        }

        [Fact]
        public void WhenCreatingFilterInfoWithNullParameterThenThrowsException()
        {   
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FilterInfo(null, FilterScope.Global), "instance");
        }
    }
}