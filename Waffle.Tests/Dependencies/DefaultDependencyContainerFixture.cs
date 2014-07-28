namespace Waffle.Tests.Dependencies
{
    using System.Diagnostics;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Waffle.Dependencies;


    [TestClass]
    public class DefaultDependencyContainerFixture
    {
        private readonly Dictionary<Type, Func<object>> store = new Dictionary<Type, Func<object>>();

        public DefaultDependencyContainerFixture()
        {
        }

        [TestMethod]
        public void RegisterInstance_InstanceIsInStore()
        {
            // Assign
            DefaultDependencyContainer container = new DefaultDependencyContainer(this.store);
            MainClass instance = new MainClass();

            // Act
            container.RegisterInstance(typeof(IMainClass), instance);

            // Assert
            Assert.AreEqual(1, this.store.Count);
        }

        [TestMethod]
        public void RegisterType_InstanceNotInStore()
        {
            // Assign
            DefaultDependencyContainer container = new DefaultDependencyContainer(this.store);

            // Act
            container.RegisteType(typeof(IMainClass), typeof(MainClass));

            // Assert
            Assert.AreEqual(0, this.store.Count);
        }


        [TestMethod]
        public void RegisterType_Multipe_InstanceNotInStore()
        {
            // Assign
            DefaultDependencyContainer container = new DefaultDependencyContainer(this.store);

            // Act
            container.RegisteType(typeof(IMainClass), typeof(MainClass));
            container.RegisteType(typeof(IMainClass), typeof(MainClass2));
            container.RegisteType(typeof(IMainClass), typeof(MainClass3));

            // Assert
            Assert.AreEqual(0, this.store.Count);
        }

        [TestMethod]
        public void RegisterType_Resolve_InstanceIsInStore()
        {
            // Assign
            DefaultDependencyContainer container = new DefaultDependencyContainer(this.store);

            // Act
            container.RegisteType(typeof(IMainClass), typeof(MainClass));
            container.RegisteType(typeof(ISubClass1), typeof(SubClass1));
            container.RegisteType(typeof(ISubClass2), typeof(SubClass2));

            Assert.AreEqual(0, this.store.Count);
            container.Resolve(typeof(IMainClass));

            // Assert
            Assert.AreEqual(3, this.store.Count);
        }


        [TestMethod]
        public void RegisterType_Multiple_Resolve_InstanceIsInStore()
        {
            // Assign
            DefaultDependencyContainer container = new DefaultDependencyContainer(this.store);

            // Act
            container.RegisteType(typeof(IMainClass), typeof(MainClass));
            container.RegisteType(typeof(IMainClass), typeof(MainClass2));
            container.RegisteType(typeof(IMainClass), typeof(MainClass3));
            container.RegisteType(typeof(ISubClass1), typeof(SubClass1));
            container.RegisteType(typeof(ISubClass2), typeof(SubClass2));

            Assert.AreEqual(0, this.store.Count);
            container.ResolveAll(typeof(IMainClass));

            // Assert
            Assert.AreEqual(5, container.Registrations.Count);
        }

        [TestMethod]
        public void Resolve_ReturnsInstance()
        {
            // Assign
            DefaultDependencyContainer container = new DefaultDependencyContainer();
            container.RegisteType(typeof(ISubClass1), typeof(SubClass1));
            container.RegisteType(typeof(IMainClass), typeof(MainClass));
            container.RegisteType(typeof(ISubClass2), typeof(SubClass2));

            // Act
            var result = container.Resolve(typeof(IMainClass));

            // Assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void Resolve_ConcreteClass_ReturnsInstance()
        {
            // Assign
            DefaultDependencyContainer container = new DefaultDependencyContainer();
            container.RegisteType(typeof(ISubClass1), typeof(SubClass1));
            container.RegisteType(typeof(IMainClass), typeof(MainClass));
            container.RegisteType(typeof(ISubClass2), typeof(SubClass2));

            // Act
            var result = container.Resolve(typeof(MainClass));

            // Assert
            Assert.IsNotNull(result);
        }
        
        [TestMethod]
        public void ResolveAll_ReturnsInstance()
        {
            // Assign
            DefaultDependencyContainer container = new DefaultDependencyContainer();
            container.RegisteType(typeof(ISubClass1), typeof(SubClass1));
            container.RegisteType(typeof(IMainClass), typeof(MainClass));
            container.RegisteType(typeof(IMainClass), typeof(MainClass2));
            container.RegisteType(typeof(IMainClass), typeof(MainClass3));
            container.RegisteType(typeof(ISubClass2), typeof(SubClass2));

            // Act
            var result = container.ResolveAll(typeof(IMainClass));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public void PerfTestCustomIoc()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < 100; i++)
            {
                Resolve_ReturnsInstance();
            }

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds + " ms");
        }
        [TestMethod]
        public void PerfTestUnity()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < 100; i++)
            {
                PerfTestUnity_X();
            }

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds + " ms");
        }

        [TestMethod]
        public void PerfTestUnity_X()
        {
            // Assign
            UnityContainer container = new UnityContainer();
            
            container.RegisterType(typeof(ISubClass1), typeof(SubClass1), new TransientLifetimeManager());
            container.RegisterType(typeof(IMainClass), typeof(MainClass), new TransientLifetimeManager());
            container.RegisterType(typeof(ISubClass2), typeof(SubClass2), new TransientLifetimeManager());

            // Act
            var result = container.Resolve(typeof(IMainClass));

            // Assert
            Assert.IsNotNull(result);
        }


        interface IMainClass
        {
        }

        private class MainClass : IMainClass
        {
            public MainClass(ISubClass1 subClass)
            {
            }
            public MainClass()
            {
            }
        }
        private class MainClass2 : IMainClass
        {
            public MainClass2(ISubClass1 subClass)
            {
            }
            public MainClass2()
            {
            }
        }
        private class MainClass3 : IMainClass
        {
            public MainClass3(ISubClass1 subClass)
            {
            }
            public MainClass3()
            {
            }
        }

        interface ISubClass1
        {
        }

        private class SubClass1 : ISubClass1
        {
            public SubClass1(ISubClass2 subClass)
            {
            }
        }

        interface ISubClass2
        {
        }

        private class SubClass2 : ISubClass2
        {
            public SubClass2()
            {
            }
        }
    }
}
