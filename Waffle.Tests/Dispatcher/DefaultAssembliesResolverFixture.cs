namespace Waffle.Tests.Dispatcher
{
    using System.Collections.Generic;
    using System.Reflection;
    using Waffle;
    using Xunit;

    
    public class DefaultAssembliesResolverFixture
    {
        [Fact]
        public void WhenGettingAssembliesThenReturnsAssemblies()
        {
            // Assign
            IAssembliesResolver resolver = new DefaultAssembliesResolver();

            // Act
            ICollection<Assembly> assemblies = resolver.GetAssemblies();

            // Assert
            Assert.NotNull(assemblies);
        }
    }
}