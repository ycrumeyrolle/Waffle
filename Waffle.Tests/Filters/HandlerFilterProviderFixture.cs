namespace Waffle.Tests.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Waffle;
    using Xunit;
    using Moq;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;

    
    public sealed class HandlerFilterProviderFixture : IDisposable
    {
        private readonly HandlerFilterProvider provider = new HandlerFilterProvider();

        private readonly ProcessorConfiguration configuration = new ProcessorConfiguration();

        [Fact]
        public void WhenGettingFiltersIfConfigurationParameterIsNullThenThrowsException()
        {
            ExceptionAssert.ThrowsArgumentNull(() => this.provider.GetFilters(null, new Mock<CommandHandlerDescriptor>().Object), "configuration");
        }

        [Fact]
        public void WhenGettingFiltersIfDescriptorParameterIsNullThenThrowsException()
        {
            ExceptionAssert.ThrowsArgumentNull(() => this.provider.GetFilters(this.configuration, null), "descriptor");
        }

        [Fact]
        public void WhenGettingFiltersFromHandlerDecriptorThenReturnsCollection()
        {
            // Arrange
            var comparer = new TestFilterInfoComparer();
            Mock<CommandHandlerDescriptor> descriptor = new Mock<CommandHandlerDescriptor>();
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
            Assert.Equal(2, result.Count);
            Assert.True(comparer.Equals(new FilterInfo(filter1, FilterScope.Handler), result[0]));
            Assert.True(comparer.Equals(new FilterInfo(filter2, FilterScope.Handler), result[1]));
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

        public void Dispose()
        {
            this.configuration.Dispose();
        }
    }
}