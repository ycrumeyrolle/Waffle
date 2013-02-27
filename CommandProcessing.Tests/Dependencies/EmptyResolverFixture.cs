namespace CommandProcessing.Tests.Dependencies
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using CommandProcessing;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Descriptions;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EmptyResolverFixture
    {
        [TestMethod]
        public void WhenBeginningScopeThenReturnsItself()
        {
            // Assign
            IDependencyResolver resolver = EmptyResolver.Instance;

            // Act
            IDependencyScope scope = resolver.BeginScope();

            // Assert
            Assert.AreSame(resolver, scope);
        }

        public void WhenGettingAnyServiceThenReturnsNull(Type serviceType)
        {
            // Assign
            IDependencyResolver resolver = EmptyResolver.Instance;

            // Act
            object service = resolver.GetService(serviceType);

            // Assert
            Assert.IsNull(service);
        }

        [TestMethod]
        public void WhenGettingAnyServiceThenReturnsNull()
        {
            foreach (Type type in KnowTypes.Concat(AnyTypes).Select(t => t[0]))
            {
                this.WhenGettingAnyServiceThenReturnsNull(type);
            }
        }
        
        public void WhenGettingAnyServiceThenReturnsEmptySequence(Type serviceType)
        {
            // Assign
            IDependencyResolver resolver = EmptyResolver.Instance;

            // Act
            IEnumerable<object> services = resolver.GetServices(serviceType);

            // Assert
            Assert.IsNotNull(services);
            Assert.AreEqual(0, services.Count());
        }

        [TestMethod]
        public void WhenGettingAnyServicesThenReturnsEmptySequence()
        {
            foreach (Type type in KnowTypes.Concat(AnyTypes).Select(t => t[0]))
            {
                this.WhenGettingAnyServiceThenReturnsEmptySequence(type);
            }
        }

        public static IEnumerable<object[]> KnowTypes
        {
            get
            {
                yield return new object[] { typeof(IHandlerSelector) };
                yield return new object[] { typeof(IHandlerActivator) };
                yield return new object[] { typeof(IHandlerTypeResolver) };
                yield return new object[] { typeof(IHandlerNameResolver) };
                yield return new object[] { typeof(IFilterProvider) };
                yield return new object[] { typeof(IAssembliesResolver) };
                yield return new object[] { typeof(ICommandExplorer) };
            }
        }

        public static IEnumerable<object[]> AnyTypes
        {
            get
            {
                yield return new object[] { typeof(IDisposable) };
                yield return new object[] { typeof(ICollection) };
                yield return new object[] { typeof(IHandlerFilter) };
            }
        }
    }
}