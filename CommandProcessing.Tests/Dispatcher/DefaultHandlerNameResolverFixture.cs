namespace CommandProcessing.Tests.Dispatcher
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using CommandProcessing;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DefaultHandlerNameResolverFixture
    {
        [TestMethod]
        public void WhenGettingHandlerNameFromAttributeThenReturnsAttributeValue()
        {
            // Assign
            DefaultHandlerNameResolver resolver = new DefaultHandlerNameResolver();
            HandlerDescriptor descriptor = new HandlerDescriptor(new ProcessorConfiguration(), typeof(HandlerWithDisplayAttribute));

            // Act
            string name = resolver.GetHandlerName(descriptor);

            // Assert
            Assert.IsNotNull(name);
            Assert.AreEqual("Name from DisplayAttribute", name);
        }

        [TestMethod]
        public void WhenGettingHandlerNameFromTypeThenReturnsTypeName()
        {
            // Assign
            DefaultHandlerNameResolver resolver = new DefaultHandlerNameResolver();
            HandlerDescriptor descriptor = new HandlerDescriptor(new ProcessorConfiguration(), typeof(HandlerWithoutDisplayAttribute));

            // Act
            string name = resolver.GetHandlerName(descriptor);

            // Assert
            Assert.IsNotNull(name);
            Assert.AreEqual(typeof(HandlerWithoutDisplayAttribute).Name, name);
        }

        [DisplayName("Name from DisplayAttribute")]
        private class HandlerWithDisplayAttribute : Handler<ICommand>
        {

            public override void Handle(ICommand command)
            {
                throw new System.NotImplementedException();
            }
        }

        private class HandlerWithoutDisplayAttribute : Handler<ICommand>
        {
            public override void Handle(ICommand command)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}