namespace CommandProcessing.Tests.Filters
{
    using System.Linq;

    using CommandProcessing;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;
    using CommandProcessing.Tests.Helpers;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

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