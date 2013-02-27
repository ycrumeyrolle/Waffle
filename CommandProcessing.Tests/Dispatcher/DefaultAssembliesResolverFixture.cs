namespace CommandProcessing.Tests.Dispatcher
{
    using System.Collections.Generic;
    using System.Reflection;
    using CommandProcessing;
    using CommandProcessing.Dispatcher;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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