namespace CommandProcessing.Tests.Services
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using CommandProcessing;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;
    using CommandProcessing.Services;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HandlerServicesFixture
    {

        [TestMethod]
        public void Ctor_Guard()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new HandlerServices(null), "parent");
        }

        [TestMethod]
        public void WhenGettingServiceWithoutOverrideThenReturnsSameAsOriginal()
        {
            ProcessorConfiguration config = new ProcessorConfiguration();
            HandlerServices services = new HandlerServices(config.Services);

            // Act
            IHandlerNameResolver localVal = (IHandlerNameResolver)services.GetService(typeof(IHandlerNameResolver));
            IHandlerNameResolver globalVal = (IHandlerNameResolver)config.Services.GetService(typeof(IHandlerNameResolver));

            // Assert
            // Local handler didn't override, should get same value as global case.
            Assert.AreSame(localVal, globalVal);
        }

        [TestMethod]
        public void WhenGettingServicesWithoutOverrideThenReturnsSameAsOriginals()
        {
            ProcessorConfiguration config = new ProcessorConfiguration();
            HandlerServices services = new HandlerServices(config.Services);

            // Act
            var localVal = services.GetServices(typeof(IFilterProvider));
            var globalVal = config.Services.GetServices(typeof(IFilterProvider));

            // Assert
            // Local handler didn't override, should get same value as global case.
            CollectionAssert.AreEqual(localVal.ToArray(), globalVal.ToArray());
        }

       [TestMethod]
        public void WhenGettingServiceWithOverrideThenReturnsOverride()
       {
           ProcessorConfiguration config = new ProcessorConfiguration();
           HandlerServices services = new HandlerServices(config.Services);

           IHandlerNameResolver newLocalService = new Mock<IHandlerNameResolver>().Object;
           services.Replace(typeof(IHandlerNameResolver), newLocalService);

           // Act            
           IHandlerNameResolver localVal = (IHandlerNameResolver)services.GetService(typeof(IHandlerNameResolver));
           IHandlerNameResolver globalVal = (IHandlerNameResolver)config.Services.GetService(typeof(IHandlerNameResolver));

           // Assert
           // Local handler didn't override, should get same value as global case.
           Assert.AreSame(localVal, newLocalService);
           Assert.AreNotSame(localVal, globalVal);
       }

       [TestMethod]
       public void WhenGettingServiceWithDependencyInjectionThenReturnsFromDependencyInjection()
       {
           // Setting on Controller config overrides the DI container. 
           ProcessorConfiguration config = new ProcessorConfiguration();

           IHandlerNameResolver newDiService = new Mock<IHandlerNameResolver>().Object;
           var mockDependencyResolver = new Mock<IDependencyResolver>();
           mockDependencyResolver.Setup(dr => dr.GetService(typeof(IHandlerNameResolver))).Returns(newDiService);
           config.DependencyResolver = mockDependencyResolver.Object;

           HandlerServices services = new HandlerServices(config.Services);

           IHandlerNameResolver newLocalService = new Mock<IHandlerNameResolver>().Object;
           services.Replace(typeof(IHandlerNameResolver), newLocalService);

           // Act            
           IHandlerNameResolver localVal = (IHandlerNameResolver)services.GetService(typeof(IHandlerNameResolver));
           IHandlerNameResolver globalVal = (IHandlerNameResolver)config.Services.GetService(typeof(IHandlerNameResolver));

           // Assert
           // Local handler didn't override, should get same value as global case.            
           Assert.AreSame(newDiService, globalVal); // asking the config will give back the DI service
           Assert.AreSame(newLocalService, localVal); // but asking locally will get back the local service.
       }

       [TestMethod]
       public void WhenAddingServiceToOverrideThenOriginalIsNotMutated()
       {
           // Controller Services has "copy on write" semantics for inherited list. 
           // It can get the inherited list and mutate it. 
           ProcessorConfiguration config = new ProcessorConfiguration();
           ServicesContainer global = config.Services;

           HandlerServices services = new HandlerServices(global);

           IFilterProvider resolver = new Mock<IFilterProvider>().Object;

           // Act
           services.Add(typeof(IFilterProvider), resolver); // appends to end

           // Assert
           IEnumerable<object> original = global.GetServices(typeof(IFilterProvider));
           object[] modified = services.GetServices(typeof(IFilterProvider)).ToArray();

           Assert.IsTrue(original.Count() > 1);
           object[] expected = original.Concat(new object[] { resolver }).ToArray();
           CollectionAssert.AreEqual(expected, modified);
       }

       [TestMethod]
       public void WhenClearingOverridedServiceThenReturnsOriginal()
       {
           ProcessorConfiguration config = new ProcessorConfiguration();
           ServicesContainer global = config.Services;

           HandlerServices services = new HandlerServices(global);
           IHandlerNameResolver newLocalService = new Mock<IHandlerNameResolver>().Object;
           services.Replace(typeof(IHandlerNameResolver), newLocalService);

           // Act
           services.Clear(typeof(IHandlerNameResolver));

           // Assert
           IHandlerNameResolver localVal = (IHandlerNameResolver)services.GetService(typeof(IHandlerNameResolver));
           IHandlerNameResolver globalVal = (IHandlerNameResolver)global.GetService(typeof(IHandlerNameResolver));

           Assert.AreSame(globalVal, localVal);
       }

       [TestMethod]
       public void WhenSettingOverrideToNullThenReturnsNull()
       {
           ProcessorConfiguration config = new ProcessorConfiguration();
           ServicesContainer global = config.Services;

           HandlerServices services = new HandlerServices(global);

           // Act
           // Setting to null is not the same as clear. Clear() means fall through to global config. 
           services.Replace(typeof(IHandlerNameResolver), null);

           // Assert
           IHandlerNameResolver localVal = (IHandlerNameResolver)services.GetService(typeof(IHandlerNameResolver));

           Assert.IsNull(localVal);
       }
    }
}