namespace Waffle.Tests.Filters
{
    using System.Linq;
    using Xunit;
    using Moq;
    using Waffle;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;

    
    public class ConfigurationFilterProviderFixture
    {
        private readonly ConfigurationFilterProvider provider = new ConfigurationFilterProvider();

        [Fact]
        public void WhenGettingWithParameterNullThenThrowsException()
        {
            ExceptionAssert.ThrowsArgumentNull(() => this.provider.GetFilters(null, null), "configuration");
        }

        [Fact]
        public void WhenGettingFiltersThenReturnsFiltersFromConfiguration()
        {
            ProcessorConfiguration config = new ProcessorConfiguration();
            IFilter filter = new Mock<IFilter>().Object;
            config.Filters.Add(filter);

            var result = this.provider.GetFilters(config, null);

            Assert.NotNull(result);
            Assert.True(result.All(f => f.Scope == FilterScope.Global));
            Assert.Same(filter, result.ToArray()[0].Instance);
        }
    }
}