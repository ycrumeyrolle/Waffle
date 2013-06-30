namespace Waffle.Tests.Dispatcher
{
    using System.Collections.Generic;
    using System.Reflection;
    using Waffle;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Waffle.Dispatcher;

    [TestClass]
    public class DefaultAssembliesResolverFixture
    {
        [TestMethod]
        public void WhenGettingAssembliesThenReturnsAssemblies()
        {
            // Assign
            IAssembliesResolver resolver = new DefaultAssembliesResolver();

            // Act
            ICollection<Assembly> assemblies = resolver.GetAssemblies();

            // Assert
            Assert.IsNotNull(assemblies);
        }
    }
}