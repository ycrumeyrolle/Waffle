// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Moq;
using CommandProcessing.Filters;
using MvcApplication6.Tests;
using Xunit;

namespace MvcApplication6.Tests
{
    public class FilterInfoTest
    {
        [Fact]
        public void Constructor()
        {
            var filterInstance = new Mock<IFilter>().Object;

            FilterInfo filter = new FilterInfo(filterInstance, FilterScope.Handler);

            Assert.Equal(FilterScope.Handler, filter.Scope);
            Assert.Same(filterInstance, filter.Instance);
        }

        [Fact]
        public void Constructor_IfInstanceParameterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                new FilterInfo(instance: null, scope: FilterScope.Handler);
            }, "instance");
        }
    }
}
