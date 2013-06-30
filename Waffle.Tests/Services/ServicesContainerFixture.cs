namespace Waffle.Tests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Waffle;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Dependencies;
    using Waffle.Dispatcher;
    using Waffle.Filters;
    using Waffle.Interception;
    using Waffle.Services;
    using Waffle.Tests.Helpers;
    using Waffle.Validation;

    [TestClass]
    public class DefaultServicesFixture
    {
        [TestMethod]
        public void Constructor_GuardClauses()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => new DefaultServices(null), "configuration");
        }

        [TestMethod]
        public void Constructor_DefaultServicesInContainer()
        {
            // Arrange
            var config = new ProcessorConfiguration();

            // Act
            var defaultServices = new DefaultServices(config);

            // Assert
            Assert.IsInstanceOfType(defaultServices.GetService(typeof(IHandlerSelector)), typeof(DefaultHandlerSelector));
            Assert.IsInstanceOfType(defaultServices.GetService(typeof(IHandlerActivator)), typeof(DefaultHandlerActivator));
            Assert.IsInstanceOfType(defaultServices.GetService(typeof(IHandlerTypeResolver)), typeof(DefaultHandlerTypeResolver));
            Assert.IsInstanceOfType(defaultServices.GetService(typeof(IAssembliesResolver)), typeof(DefaultAssembliesResolver));
            Assert.IsInstanceOfType(defaultServices.GetService(typeof(IInterceptionProvider)), typeof(DefaultInterceptionProvider));

            object[] filterProviders = defaultServices.GetServices(typeof(IFilterProvider)).ToArray();
            Assert.AreEqual(2, filterProviders.Length);
            Assert.IsInstanceOfType(filterProviders[0], typeof(ConfigurationFilterProvider));
            Assert.IsInstanceOfType(filterProviders[1], typeof(HandlerFilterProvider));

            Assert.IsInstanceOfType(defaultServices.GetService(typeof(ICommandValidator)), typeof(DefaultCommandValidator));

            object[] interceptors = defaultServices.GetServices(typeof(IInterceptor)).ToArray();
            Assert.AreEqual(0, interceptors.Length);
        }

        [TestMethod]
        public void Add_GuardClauses()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.Add(serviceType: null, service: new object()), "serviceType");
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.Add(typeof(object), service: null), "service");
            ExceptionAssert.ThrowsArgument(() => defaultServices.Add(typeof(object), new object()), "serviceType");
            ExceptionAssert.ThrowsArgument(() => defaultServices.Add(typeof(IFilterProvider), new object()), "service");
        }

        [TestMethod]
        public void Add_AddsServiceToEndOfServicesList()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider = new Mock<IFilterProvider>().Object;
            IEnumerable<object> servicesBefore = defaultServices.GetServices(typeof(IFilterProvider));

            // Act
            defaultServices.Add(typeof(IFilterProvider), filterProvider);

            // Assert
            IEnumerable<object> servicesAfter = defaultServices.GetServices(typeof(IFilterProvider));
            CollectionAssert.AreEqual(servicesBefore.Concat(new[] { filterProvider }).ToArray(), servicesAfter.ToArray());
        }

        [TestMethod]
        public void AddRange_GuardClauses()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.AddRange(serviceType: null, services: new[] { new object() }), "serviceType");
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.AddRange(typeof(object), services: null), "services");
            ExceptionAssert.ThrowsArgument(() => defaultServices.AddRange(typeof(object), new[] { new object() }), "serviceType");
            ExceptionAssert.ThrowsArgument(() => defaultServices.AddRange(typeof(IFilterProvider), new[] { new object() }), "services");
        }

        [TestMethod]
        public void AddRange_AddsServicesToEndOfServicesList()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider = new Mock<IFilterProvider>().Object;
            IEnumerable<object> servicesBefore = defaultServices.GetServices(typeof(IFilterProvider));

            // Act
            defaultServices.AddRange(typeof(IFilterProvider), new[] { filterProvider });

            // Assert
            IEnumerable<object> servicesAfter = defaultServices.GetServices(typeof(IFilterProvider));
            CollectionAssert.AreEqual(servicesBefore.Concat(new[] { filterProvider }).ToArray(), servicesAfter.ToArray());
        }

        [TestMethod]
        public void AddRange_SkipsNullObjects()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            IEnumerable<object> servicesBefore = defaultServices.GetServices(typeof(IFilterProvider));

            // Act
            defaultServices.AddRange(typeof(IFilterProvider), new object[] { null });

            // Assert
            IEnumerable<object> servicesAfter = defaultServices.GetServices(typeof(IFilterProvider));
            CollectionAssert.AreEqual(servicesBefore.ToArray(), servicesAfter.ToArray());
        }

        [TestMethod]
        public void Clear_GuardClauses()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.Clear(serviceType: null), "serviceType");
            ExceptionAssert.ThrowsArgument(() => defaultServices.Clear(typeof(object)), "serviceType");
        }

        [TestMethod]
        public void Clear_RemovesAllServices()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act
            defaultServices.Clear(typeof(IFilterProvider));

            // Assert
            Assert.AreEqual(0, defaultServices.GetServices(typeof(IFilterProvider)).Count());
        }

        [TestMethod]
        public void FindIndex_GuardClauses()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.FindIndex(serviceType: null, match: _ => true), "serviceType");
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.FindIndex(typeof(object), match: null), "match");
            ExceptionAssert.ThrowsArgument(() => defaultServices.FindIndex(typeof(object), _ => true), "serviceType");
        }

        [TestMethod]
        public void FindIndex_SuccessfulFind()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act
            int index = defaultServices.FindIndex(typeof(IFilterProvider), _ => true);

            // Assert
            Assert.AreEqual(0, index);
        }

        [TestMethod]
        public void FindIndex_FailedFind()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act
            int index = defaultServices.FindIndex(typeof(IFilterProvider), _ => false);

            // Assert
            Assert.AreEqual(-1, index);
        }

        [TestMethod]
        public void FindIndex_EmptyServiceListAlwaysReturnsFailure()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            defaultServices.Clear(typeof(IFilterProvider));

            // Act
            int index = defaultServices.FindIndex(typeof(IFilterProvider), _ => true);

            // Assert
            Assert.AreEqual(-1, index);
        }

        [TestMethod]
        public void GetService_GuardClauses()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.GetService(serviceType: null), "serviceType");
            ExceptionAssert.ThrowsArgument(() => defaultServices.GetService(typeof(object)), "serviceType");
        }

        [TestMethod]
        public void GetService_ReturnsNullWhenServiceListEmpty()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            defaultServices.Clear(typeof(IHandlerSelector));

            // Act
            object service = defaultServices.GetService(typeof(IHandlerSelector));

            // Assert
            Assert.IsNull(service);
        }

        [TestMethod]
        public void GetService_PrefersServiceInDependencyInjectionContainer()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider = new Mock<IHandlerSelector>().Object;
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            mockDependencyResolver.Setup(dr => dr.GetService(typeof(IHandlerSelector))).Returns(filterProvider);
            config.DependencyResolver = mockDependencyResolver.Object;

            // Act
            object service = defaultServices.GetService(typeof(IHandlerSelector));

            // Assert
            Assert.AreSame(filterProvider, service);
        }

        [TestMethod]
        public void GetService_CachesResultFromDependencyInjectionContainer()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            config.DependencyResolver = mockDependencyResolver.Object;

            // Act
            defaultServices.GetService(typeof(IHandlerSelector));
            defaultServices.GetService(typeof(IHandlerSelector));

            // Assert
            mockDependencyResolver.Verify(dr => dr.GetService(typeof(IHandlerSelector)), Times.Once());
        }

        [TestMethod]
        public void GetServices_GuardClauses()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.GetServices(serviceType: null), "serviceType");
            ExceptionAssert.ThrowsArgument(() => defaultServices.GetServices(typeof(object)), "serviceType");
        }

        [TestMethod]
        public void GetServices_ReturnsEmptyEnumerationWhenServiceListEmpty()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            defaultServices.Clear(typeof(IFilterProvider));

            // Act
            IEnumerable<object> services = defaultServices.GetServices(typeof(IFilterProvider));

            // Assert
            Assert.AreEqual(0, services.Count());
        }

        [TestMethod]
        public void GetServices_PrependsServiceInDependencyInjectionContainer()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            IEnumerable<object> servicesBefore = defaultServices.GetServices(typeof(IFilterProvider));
            var filterProvider = new Mock<IFilterProvider>().Object;
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            mockDependencyResolver.Setup(dr => dr.GetServices(typeof(IFilterProvider))).Returns(new[] { filterProvider });
            config.DependencyResolver = mockDependencyResolver.Object;

            // Act
            IEnumerable<object> servicesAfter = defaultServices.GetServices(typeof(IFilterProvider));

            // Assert
            CollectionAssert.AreEqual(new[] { filterProvider }.Concat(servicesBefore).ToArray(), servicesAfter.ToArray());
        }

        [TestMethod]
        public void GetServices_CachesResultFromDependencyInjectionContainer()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            config.DependencyResolver = mockDependencyResolver.Object;

            // Act
            defaultServices.GetServices(typeof(IFilterProvider));
            defaultServices.GetServices(typeof(IFilterProvider));

            // Assert
            mockDependencyResolver.Verify(dr => dr.GetServices(typeof(IFilterProvider)), Times.Once());
        }

        [TestMethod]
        public void Insert_GuardClauses()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.Insert(serviceType: null, index: 0, service: new object()), "serviceType");
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.Insert(typeof(object), 0, service: null), "service");
            ExceptionAssert.ThrowsArgument(() => defaultServices.Insert(typeof(object), 0, new object()), "serviceType");
            ExceptionAssert.ThrowsArgument(() => defaultServices.Insert(typeof(IFilterProvider), 0, new object()), "service");
            ExceptionAssert.ThrowsArgumentOutOfRange(() => defaultServices.Insert(typeof(IFilterProvider), -1, new Mock<IFilterProvider>().Object), "index");
        }

        [TestMethod]
        public void Insert_AddsElementAtTheRequestedLocation()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            var newFilterProvider = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.Insert(typeof(IFilterProvider), 1, newFilterProvider);

            // Assert
            CollectionAssert.AreEqual(new[] { filterProvider1, newFilterProvider, filterProvider2 }.ToArray(), defaultServices.GetServices(typeof(IFilterProvider)).ToArray());
        }

        [TestMethod]
        public void InsertRange_GuardClauses()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.InsertRange(serviceType: null, index: 0, services: new[] { new object() }), "serviceType");
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.InsertRange(typeof(object), 0, services: null), "services");
            ExceptionAssert.ThrowsArgument(() => defaultServices.InsertRange(typeof(object), 0, new[] { new object() }), "serviceType");
            ExceptionAssert.ThrowsArgument(() => defaultServices.InsertRange(typeof(IFilterProvider), 0, new[] { new object() }), "services");
            ExceptionAssert.ThrowsArgumentOutOfRange(() => defaultServices.InsertRange(typeof(IFilterProvider), -1, new[] { new Mock<IFilterProvider>().Object }), "index");
        }

        [TestMethod]
        public void InsertRange_AddsElementAtTheRequestedLocation()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            var newFilterProvider1 = new Mock<IFilterProvider>().Object;
            var newFilterProvider2 = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.InsertRange(typeof(IFilterProvider), 1, new[] { newFilterProvider1, newFilterProvider2 });

            // Assert
            CollectionAssert.AreEqual(new[] { filterProvider1, newFilterProvider1, newFilterProvider2, filterProvider2 }.ToArray(), defaultServices.GetServices(typeof(IFilterProvider)).ToArray());
        }

        [TestMethod]
        public void Remove_GuardClauses()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.Remove(serviceType: null, service: new object()), "serviceType");
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.Remove(typeof(object), service: null), "service");
            ExceptionAssert.ThrowsArgument(() => defaultServices.Remove(typeof(object), new object()), "serviceType");
        }

        [TestMethod]
        public void Remove_ObjectFound()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.Remove(typeof(IFilterProvider), filterProvider1);

            // Assert
            CollectionAssert.AreEqual(new[] { filterProvider2 }.ToArray(), defaultServices.GetServices(typeof(IFilterProvider)).ToArray());
        }

        [TestMethod]
        public void Remove_ObjectNotFound()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            var notPresentFilterProvider = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.Remove(typeof(IFilterProvider), notPresentFilterProvider);

            // Assert
            CollectionAssert.AreEqual(new[] { filterProvider1, filterProvider2 }.ToArray(), defaultServices.GetServices(typeof(IFilterProvider)).ToArray());
        }

        [TestMethod]
        public void RemoveAll_GuardClauses()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.RemoveAll(serviceType: null, match: _ => true), "serviceType");
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.RemoveAll(typeof(object), match: null), "match");
            ExceptionAssert.ThrowsArgument(() => defaultServices.RemoveAll(typeof(object), _ => true), "serviceType");
        }

        [TestMethod]
        public void RemoveAll_SuccessfulMatch()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.RemoveAll(typeof(IFilterProvider), _ => true);

            // Assert
            Assert.AreEqual(0, defaultServices.GetServices(typeof(IFilterProvider)).Count());
        }

        [TestMethod]
        public void RemoveAll_PartialMatch()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.RemoveAll(typeof(IFilterProvider), obj => obj == filterProvider2);

            // Assert
            CollectionAssert.AreEqual(new[] { filterProvider1 }.ToArray(), defaultServices.GetServices(typeof(IFilterProvider)).ToArray());
        }

        [TestMethod]
        public void RemoveAt_GuardClauses()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.RemoveAt(serviceType: null, index: 0), "serviceType");
            ExceptionAssert.ThrowsArgument(() => defaultServices.RemoveAt(typeof(object), 0), "serviceType");
            ExceptionAssert.ThrowsArgumentOutOfRange(() => defaultServices.RemoveAt(typeof(IFilterProvider), -1), "index");
        }

        [TestMethod]
        public void RemoteAt_RemovesService()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.RemoveAt(typeof(IFilterProvider), 1);

            // Assert
            CollectionAssert.AreEqual(new[] { filterProvider1 }, defaultServices.GetServices(typeof(IFilterProvider)).ToArray());
        }

        [TestMethod]
        public void Replace_GuardClauses()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.Replace(serviceType: null, service: new object()), "serviceType");
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.Replace(typeof(IFilterProvider), service: null), "service");
            ExceptionAssert.ThrowsArgument(() => defaultServices.Replace(typeof(object), new object()), "serviceType");
            ExceptionAssert.ThrowsArgument(() => defaultServices.Replace(typeof(IFilterProvider), new object()), "service");
        }

        [TestMethod]
        public void Replace_ReplacesAllValuesWithTheGivenService()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            var newFilterProvider = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.Replace(typeof(IFilterProvider), newFilterProvider);

            // Assert
            CollectionAssert.AreEqual(new[] { newFilterProvider }, defaultServices.GetServices(typeof(IFilterProvider)).ToArray());
        }

        [TestMethod]
        public void ReplaceRange_GuardClauses()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.ReplaceRange(serviceType: null, services: new[] { new object() }), "serviceType");
            ExceptionAssert.ThrowsArgumentNull(() => defaultServices.ReplaceRange(typeof(object), services: null), "services");
            ExceptionAssert.ThrowsArgument(() => defaultServices.ReplaceRange(typeof(object), new[] { new object() }), "serviceType");
            ExceptionAssert.ThrowsArgument(() => defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { new object() }), "services");
        }

        [TestMethod]
        public void ReplaceRange_ReplacesAllValuesWithTheGivenServices()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;

            // Act
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Assert
            CollectionAssert.AreEqual(new[] { filterProvider1, filterProvider2 }, defaultServices.GetServices(typeof(IFilterProvider)).ToArray());
        }
    }
}