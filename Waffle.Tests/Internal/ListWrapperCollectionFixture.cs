// -----------------------------------------------------------------------
// <copyright file="ListWrapperCollectionFixture.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Waffle.Tests.Internal
{
    using System.Collections.Generic;
    using Waffle.Internal;
    using Xunit;
    using Waffle.Tests.Helpers;

    
    public class ListWrapperCollectionTests
    {
        [Fact]
        public void ListWrapperCollection_ItemsList_HasSameContents()
        {
            // Arrange
            ListWrapperCollection<object> listWrapper = new ListWrapperCollection<object>();

            // Act
            listWrapper.Add(new object());
            listWrapper.Add(new object());

            // Assert
            Assert.Equal(listWrapper, listWrapper.ItemsList);
        }

        [Fact]
        public void ListWrapperCollection_ItemsList_IsPassedInList()
        {
            // Arrange
            List<object> list = new List<object> { new object(), new object() };
            ListWrapperCollection<object> listWrapper = new ListWrapperCollection<object>(list);

            // Act & Assert
            Assert.Same(list, listWrapper.ItemsList);
        }
    }

}
