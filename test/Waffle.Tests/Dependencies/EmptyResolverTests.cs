namespace Waffle.Tests.Dependencies
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Dependencies;
    using Waffle.Filters;
    using Xunit;
    
    public class EmptyResolverTests
    {
        [Fact]
        public void WhenBeginningScopeThenReturnsItself()
        {
            // Assign
            IDependencyResolver resolver = EmptyResolver.Instance;

            // Act
            IDependencyScope scope = resolver.BeginScope();

            // Assert
            Assert.Same(resolver, scope);
        }

        [Theory]
        [MemberData("KnowTypes")]
        [MemberData("AnyTypes")]
        public void WhenGettingAnyServiceThenReturnsNull(Type serviceType)
        {
            // Assign
            IDependencyResolver resolver = EmptyResolver.Instance;

            // Act
            object service = resolver.GetService(serviceType);

            // Assert
            Assert.Null(service);
        }

        [Theory]
        [MemberData("KnowTypes")]
        [MemberData("AnyTypes")]
        public void WhenGettingAnyServiceThenReturnsEmptySequence(Type serviceType)
        {
            // Assign
            IDependencyResolver resolver = EmptyResolver.Instance;

            // Act
            IEnumerable<object> services = resolver.GetServices(serviceType);

            // Assert
            Assert.NotNull(services);
            Assert.Equal(0, services.Count());
        }
        
        public static IEnumerable<object[]> KnowTypes
        {
            get
            {
                yield return new object[] { typeof(ICommandHandlerSelector) };
                yield return new object[] { typeof(ICommandHandlerActivator) };
                yield return new object[] { typeof(ICommandHandlerTypeResolver) };
                yield return new object[] { typeof(IFilterProvider) };
                yield return new object[] { typeof(IAssembliesResolver) };
            }
        }

        public static IEnumerable<object[]> AnyTypes
        {
            get
            {
                yield return new object[] { typeof(IDisposable) };
                yield return new object[] { typeof(ICollection) };
                yield return new object[] { typeof(ICommandHandlerFilter) };
            }
        }
    }
}