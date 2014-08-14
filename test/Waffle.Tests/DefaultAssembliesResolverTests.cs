namespace Waffle.Tests
{
    using System.Collections.Generic;
    using System.Reflection;
    using Xunit;

    public class DefaultAssembliesResolverTests
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