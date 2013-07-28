namespace Waffle.Tests.Filters
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;

    [TestClass]
    public class ConfigurationFilterProviderFixture
    {
        private readonly ConfigurationFilterProvider provider = new ConfigurationFilterProvider();

        [TestMethod]
        public void WhenGettingWithParameterNullThenThrowsException()
        {
            ExceptionAssert.ThrowsArgumentNull(() => this.provider.GetFilters(null, null), "configuration");
        }

        [TestMethod]
        public void WhenGettingFiltersThenReturnsFiltersFromConfiguration()
        {
            ProcessorConfiguration config = new ProcessorConfiguration();
            IFilter filter = new Mock<IFilter>().Object;
            config.Filters.Add(filter);

            var result = this.provider.GetFilters(config, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.All(f => f.Scope == FilterScope.Global));
            Assert.AreSame(filter, result.ToArray()[0].Instance);
        }
    }
}