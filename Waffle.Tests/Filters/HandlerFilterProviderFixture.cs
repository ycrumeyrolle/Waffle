namespace Waffle.Tests.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Waffle;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;

    [TestClass]
    public sealed class HandlerFilterProviderFixture : IDisposable
    {
        private readonly HandlerFilterProvider provider = new HandlerFilterProvider();

        private readonly ProcessorConfiguration configuration = new ProcessorConfiguration();

        [TestMethod]
        public void WhenGettingFiltersIfConfigurationParameterIsNullThenThrowsException()
        {
            ExceptionAssert.ThrowsArgumentNull(() => this.provider.GetFilters(null, new Mock<HandlerDescriptor>().Object), "configuration");
        }

        [TestMethod]
        public void WhenGettingFiltersIfDescriptorParameterIsNullThenThrowsException()
        {
            ExceptionAssert.ThrowsArgumentNull(() => this.provider.GetFilters(this.configuration, null), "descriptor");
        }

        [TestMethod]
        public void WhenGettingFiltersFromHandlerDecriptorThenReturnsCollection()
        {
            // Arrange
            var comparer = new TestFilterInfoComparer();
            Mock<HandlerDescriptor> descriptor = new Mock<HandlerDescriptor>();
            IFilter filter1 = new Mock<IFilter>().Object;
            IFilter filter2 = new Mock<IFilter>().Object;
            descriptor
                .Setup(d => d.GetFilters())
                .Returns(new Collection<IFilter>(new[] { filter1, filter2 }))
                .Verifiable();

            // Act
            List<FilterInfo> result = this.provider.GetFilters(this.configuration, descriptor.Object).ToList();

            // Assert
            descriptor.Verify();
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(comparer.Equals(new FilterInfo(filter1, FilterScope.Handler), result[0]));
            Assert.IsTrue(comparer.Equals(new FilterInfo(filter2, FilterScope.Handler), result[1]));
        }

        public class TestFilterInfoComparer : IEqualityComparer<FilterInfo>
        {
            public bool Equals(FilterInfo x, FilterInfo y)
            {
                return (x == null && y == null) || (object.ReferenceEquals(x.Instance, y.Instance) && x.Scope == y.Scope);
            }

            public int GetHashCode(FilterInfo obj)
            {
                return obj.GetHashCode();
            }
        }

        [TestCleanup]
        public void Dispose()
        {
            this.configuration.Dispose();
        }
    }
}