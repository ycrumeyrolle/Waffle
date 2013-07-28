// -----------------------------------------------------------------------
// <copyright file="ListWrapperCollectionFixture.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Waffle.Tests.Internal
{
    using System.Collections.Generic;
    using Waffle.Internal;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Waffle.Tests.Helpers;

    [TestClass]
    public class ListWrapperCollectionTests
    {
        [TestMethod]
        public void ListWrapperCollection_ItemsList_HasSameContents()
        {
            // Arrange
            ListWrapperCollection<object> listWrapper = new ListWrapperCollection<object>();

            // Act
            listWrapper.Add(new object());
            listWrapper.Add(new object());

            // Assert
            EnumerableAssert.AreEqual(listWrapper, listWrapper.ItemsList);
        }

        [TestMethod]
        public void ListWrapperCollection_ItemsList_IsPassedInList()
        {
            // Arrange
            List<object> list = new List<object> { new object(), new object() };
            ListWrapperCollection<object> listWrapper = new ListWrapperCollection<object>(list);

            // Act & Assert
            Assert.AreSame(list, listWrapper.ItemsList);
        }
    }

}
