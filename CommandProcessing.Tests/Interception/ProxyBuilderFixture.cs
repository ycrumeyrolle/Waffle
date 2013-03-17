namespace CommandProcessing.Tests.Interception
{
    using System;
    using CommandProcessing.Interception;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ProxyBuilderFixture
    {
        private readonly Mock<IInterceptionProvider> interceptor;
        
        public ProxyBuilderFixture()
        {
            this.interceptor = new Mock<IInterceptionProvider>(MockBehavior.Loose);
        }

        [TestMethod]
        public void WhenBuildingProxyThenReturnsProxyOfSameType()
        {
            // Assign
            SimpleService service = new SimpleService();
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            // Act
            var proxy = builder.Build(service, this.interceptor.Object);

            // Assert
            Assert.IsNotNull(proxy);
            Assert.IsInstanceOfType(proxy, typeof(SimpleService));
        }

        [TestMethod]
        public void WhenBuildingProxyMultipleTimesThenUseCache()
        {
            // Assign
            SimpleService service = new SimpleService();
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            // Act
            var proxy = builder.Build(service, this.interceptor.Object);
            proxy = builder.Build(service, this.interceptor.Object);

            // Assert
            // TODO : How to validate the cache hit? Using a external object?
            Assert.IsNotNull(proxy);
            Assert.IsInstanceOfType(proxy, typeof(SimpleService));
        }
        
        [TestMethod]
        public void WhenCallingParameterlessServiceThenInterceptorIsCalled()
        {
            // Assign
            SimpleService service = new SimpleService();
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            this.interceptor.Setup(i => i.OnExecuting());
            this.interceptor.Setup(i => i.OnExecuted());
            this.interceptor.Setup(i => i.OnException(It.IsAny<Exception>()));
            var proxy = builder.Build(service, this.interceptor.Object);
            
            // Act
            proxy.Parameterless();

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Once());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Once());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Never());
        }

        [TestMethod]
        public void WhenCallingNonVirtualServiceThenInterceptorIsNotCalled()
        {
            // Assign
            SimpleService service = new SimpleService();
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            this.interceptor.Setup(i => i.OnExecuting());
            this.interceptor.Setup(i => i.OnExecuted());
            this.interceptor.Setup(i => i.OnException(It.IsAny<Exception>()));
            var proxy = builder.Build(service, this.interceptor.Object);

            // Act
            proxy.NonVirtual();

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Never());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Never());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Never());
        }
        
        [TestMethod]
        public void WhenCallingWithValueTypeParameterThenInterceptorIsCalled()
        {
            // Assign
            SimpleService service = new SimpleService();
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            this.interceptor.Setup(i => i.OnExecuting());
            this.interceptor.Setup(i => i.OnExecuted());
            this.interceptor.Setup(i => i.OnException(It.IsAny<Exception>()));
            var proxy = builder.Build(service, this.interceptor.Object);
            var value = 123;

            // Act 
            proxy.ValueTypeParameter(value);

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Once());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Once());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Never());

            Assert.AreEqual(value, proxy.ValueTypeValue);
        }
        
        [TestMethod]
        public void WhenCallingWithReferenceTypeParameterThenInterceptorIsCalled()
        {
            // Assign
            SimpleService service = new SimpleService();
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            this.interceptor.Setup(i => i.OnExecuting());
            this.interceptor.Setup(i => i.OnExecuted());
            this.interceptor.Setup(i => i.OnException(It.IsAny<Exception>()));
            var proxy = builder.Build(service, this.interceptor.Object);
            var value = new Random();

            // Act
            proxy.ReferenceTypeParameter(value);

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Once());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Once());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Never());
        }
        
        [TestMethod]
        public void WhenCallingWithStringParameterThenInterceptorIsCalled()
        {
            // Assign
            SimpleService service = new SimpleService();
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            this.interceptor.Setup(i => i.OnExecuting());
            this.interceptor.Setup(i => i.OnExecuted());
            this.interceptor.Setup(i => i.OnException(It.IsAny<Exception>()));
            var proxy = builder.Build(service, this.interceptor.Object);
            var value = "test123";

            // Act
            proxy.StringTypeParameter(value);

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Once());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Once());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Never());
        }
        
        [TestMethod]
        public void WhenCallingWithEnumParameterThenInterceptorIsCalled()
        {
            // Assign
            SimpleService service = new SimpleService();
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            this.interceptor.Setup(i => i.OnExecuting());
            this.interceptor.Setup(i => i.OnExecuted());
            this.interceptor.Setup(i => i.OnException(It.IsAny<Exception>()));
            var proxy = builder.Build(service, this.interceptor.Object);
            var value = StringSplitOptions.None;

            // Act
            proxy.EnumTypeParameter(value);

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Once());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Once());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Never());
        }

        [TestMethod]
        public void WhenCallingServiceReturningValueTypeThenInterceptorIsCalled()
        {
            // Assign
            SimpleService service = new SimpleService();
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            this.interceptor.Setup(i => i.OnExecuting());
            this.interceptor.Setup(i => i.OnExecuted());
            this.interceptor.Setup(i => i.OnException(It.IsAny<Exception>()));
            var proxy = builder.Build(service, this.interceptor.Object);
           
            // Act
            var value1 = proxy.ReturnsValueType();

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Once());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Once());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Never());

            Assert.AreEqual(service.ValueTypeValue, value1);
        }

        [TestMethod]
        public void WhenCallingServiceReturningReferenceTypeThenInterceptorIsCalled()
        {
            // Assign
            SimpleService service = new SimpleService();
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            this.interceptor.Setup(i => i.OnExecuting());
            this.interceptor.Setup(i => i.OnExecuted());
            this.interceptor.Setup(i => i.OnException(It.IsAny<Exception>()));
            var proxy = builder.Build(service, this.interceptor.Object);
           
            // Act
            var value2 = proxy.ReturnsReferenceType();

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Once());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Once());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Never());

            Assert.AreEqual(service.ReferenceValue, value2);
        }
        
        [TestMethod]
        public void WhenCallingServiceReturningStringTypeThenInterceptorIsCalled()
        {
            // Assign
            SimpleService service = new SimpleService();
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            this.interceptor.Setup(i => i.OnExecuting());
            this.interceptor.Setup(i => i.OnExecuted());
            this.interceptor.Setup(i => i.OnException(It.IsAny<Exception>()));
            var proxy = builder.Build(service, this.interceptor.Object);

            // Act
            var value3 = proxy.ReturnsString();

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Once());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Once());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Never());

            Assert.AreEqual(service.StringValue, value3);
        }

        [TestMethod]
        public void WhenCallingServiceReturningEnumTypeThenInterceptorIsCalled()
        {
            // Assign
            SimpleService service = new SimpleService();
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            this.interceptor.Setup(i => i.OnExecuting());
            this.interceptor.Setup(i => i.OnExecuted());
            this.interceptor.Setup(i => i.OnException(It.IsAny<Exception>()));
            var proxy = builder.Build(service, this.interceptor.Object);
            
            // Act
            var value4 = proxy.ReturnsEnum();

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Once());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Once());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Never());

            Assert.AreEqual(service.EnumValue, value4);
        }

        [TestMethod]
        public void WhenCallingServiceThrowingExceptionThenInterceptorIsCalled()
        {
            // Assign
            SimpleService service = new SimpleService();
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            this.interceptor.Setup(i => i.OnExecuting());
            this.interceptor.Setup(i => i.OnExecuted());
            this.interceptor.Setup(i => i.OnException(It.IsAny<Exception>()));
            var proxy = builder.Build(service, this.interceptor.Object);
            bool exceptionRaised = false;

            // Act
            try
            {
                proxy.ThrowsException();
            }
            catch (Exception)
            {
                exceptionRaised = true;
            }

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Once());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Never());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Once());
            Assert.IsTrue(exceptionRaised);
        }
        
        [TestMethod]
        public void WhenBuildingWithoutServiceThenThrowsArgumentNullException()
        {
            // Assign
            SimpleService service = null;
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            this.interceptor.Setup(i => i.OnExecuting());
            this.interceptor.Setup(i => i.OnExecuted());
            this.interceptor.Setup(i => i.OnException(It.IsAny<Exception>()));
            bool exceptionRaised = false;

            // Act
            try
            {
                builder.Build(service, this.interceptor.Object);
            }
            catch (ArgumentNullException)
            {
                exceptionRaised = true;
            }

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Never());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Never());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Never());
            Assert.IsTrue(exceptionRaised);
        }
        
        [TestMethod]
        public void WhenBuildingWithoutParameterlessCtorServiceThenThrowsNotSupportedException()
        {
            // Assign
            BadService service = new BadService("test");
            DefaultProxyBuilder builder = this.CreateTestableBuilder();

            this.interceptor.Setup(i => i.OnExecuting());
            this.interceptor.Setup(i => i.OnExecuted());
            this.interceptor.Setup(i => i.OnException(It.IsAny<Exception>()));
            bool exceptionRaised = false;

            // Act
            try
            {
                builder.Build(service, this.interceptor.Object);
            }
            catch (NotSupportedException)
            {
                exceptionRaised = true;
            }

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Never());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Never());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Never());
            Assert.IsTrue(exceptionRaised);
        }

        private DefaultProxyBuilder CreateTestableBuilder()
        {
            return new DefaultProxyBuilder();
        }
    }
}