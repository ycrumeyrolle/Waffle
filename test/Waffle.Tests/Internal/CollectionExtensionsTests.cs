namespace Waffle.Tests.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Waffle.Internal;
    using Xunit;

    public class CollectionExtensionsTests
    {
        [Fact]
        public void AsArray_Array_ReturnsSameInstance()
        {
            object[] array = { new object(), new object() };
            IEnumerable<object> arrayAsEnumerable = array;
            object[] arrayAsArray = arrayAsEnumerable.AsArray();

            Assert.Same(array, arrayAsArray);
        }

        [Fact]
        public void AsArray_Enumerable_Copies()
        {
            IList<object> list = new List<object> { new object(), new object() };
            object[] listToArray = list.ToArray();
            IEnumerable<object> arrayAsEnumerable = listToArray;
            object[] listAsArray = arrayAsEnumerable.AsArray();

            Assert.Equal(listAsArray, listToArray);
        }

        [Fact]
        public void AsCollection_Collection_ReturnsSameInstance()
        {
            Collection<object> collection = new Collection<object> { new object(), new object() };
            IEnumerable<object> collectionAsEnumerable = collection;
            Collection<object> collectionAsCollection = collectionAsEnumerable.AsCollection();

            Assert.Same(collection, collectionAsCollection);
        }

        [Fact]
        public void AsCollection_Enumerable_Copies()
        {
            IEnumerable<object> enumerable = new LinkedList<object>(new[] { new object(), new object() });

            Collection<object> enumerableAsCollection = enumerable.AsCollection();
            Assert.Equal(enumerable, enumerableAsCollection);
        }

        [Fact]
        public void AsCollection_IList_Wraps()
        {
            IList<object> list = new List<object> { new object(), new object() };

            Collection<object> listAsCollection = list.AsCollection();
            list.Add(new object());

            Assert.Equal(list, listAsCollection.ToList());
        }

        [Fact]
        public void AsIList_IList_ReturnsSameInstance()
        {
            List<object> list = new List<object> { new object(), new object() };
            IEnumerable<object> listAsEnumerable = list;
            IList<object> listAsIList = listAsEnumerable.AsIList();

            Assert.Same(list, listAsIList);
        }

        [Fact]
        public void AsIList_Enumerable_Copies()
        {
            LinkedList<object> enumerable = new LinkedList<object>();
            enumerable.AddLast(new object());
            enumerable.AddLast(new object());
            List<object> expected = enumerable.ToList();
            IEnumerable<object> listAsEnumerable = enumerable;
            IList<object> enumerableAsIList = listAsEnumerable.AsIList();

            Assert.Equal(expected, enumerableAsIList);
            Assert.NotSame(expected, enumerableAsIList);
        }

        [Fact]
        public void AsList_List_ReturnsSameInstance()
        {
            List<object> list = new List<object> { new object(), new object() };
            IEnumerable<object> listAsEnumerable = list;
            List<object> listAsList = listAsEnumerable.AsList();

            Assert.Same(list, listAsList);
        }

        [Fact]
        public void AsList_Enumerable_Copies()
        {
            List<object> list = new List<object> { new object(), new object() };
            object[] array = list.ToArray();
            IEnumerable<object> arrayAsEnumerable = array;
            List<object> arrayAsList = arrayAsEnumerable.AsList();

            Assert.Equal(list, arrayAsList);
            Assert.NotSame(list, arrayAsList);
            Assert.NotSame(array, arrayAsList);
        }

        [Fact]
        public void AsList_ListWrapperCollection_ReturnsSameInstance()
        {
            List<object> list = new List<object> { new object(), new object() };
            ListWrapperCollection<object> listWrapper = new ListWrapperCollection<object>(list);
            IEnumerable<object> listWrapperAsEnumerable = listWrapper;
            List<object> listWrapperAsList = listWrapperAsEnumerable.AsList();

            Assert.Same(list, listWrapperAsList);
        }

        [Fact]
        public void SingleDefaultOrErrorIListEmptyReturnsNull()
        {
            IList<object> empty = new List<object>();
            object errorArgument = new object();
            Action<object> errorAction = argument =>
            {
                throw new InvalidOperationException();
            };

            Assert.Null(empty.SingleDefaultOrError(errorAction, errorArgument));
        }

        [Fact]
        public void SingleDefaultOrErrorIListSingleReturns()
        {
            IList<object> single = new List<object> { new object() };
            object errorArgument = new object();
            Action<object> errorAction = argument =>
            {
                throw new InvalidOperationException();
            };

            Assert.Equal(single[0], single.SingleDefaultOrError(errorAction, errorArgument));
        }

        [Fact]
        public void SingleDefaultOrErrorIListMultipleThrows()
        {
            IList<object> multiple = new List<object> { new object(), new object() };
            object errorArgument = new object();
            Action<object> errorAction = argument =>
            {
                Assert.Equal(errorArgument, argument);
                throw new InvalidOperationException();
            };

            Assert.Throws<InvalidOperationException>(() => multiple.SingleDefaultOrError(errorAction, errorArgument));
        }

        [Fact]
        public void SingleOfTypeDefaultOrErrorIListNoMatchReturnsNull()
        {
            IList<object> noMatch = new List<object> { new object(), new object() };
            object errorArgument = new object();
            Action<object> errorAction = argument =>
            {
                throw new InvalidOperationException();
            };

            Assert.Null(noMatch.SingleOfTypeDefaultOrError<object, string, object>(errorAction, errorArgument));
        }

        [Fact]
        public void SingleOfTypeDefaultOrErrorIListOneMatchReturns()
        {
            IList<object> singleMatch = new List<object> { new object(), "Match", new object() };
            object errorArgument = new object();
            Action<object> errorAction = argument =>
            {
                throw new InvalidOperationException();
            };

            Assert.Equal("Match", singleMatch.SingleOfTypeDefaultOrError<object, string, object>(errorAction, errorArgument));
        }

        [Fact]
        public void SingleOfTypeDefaultOrErrorIListMultipleMatchesThrows()
        {
            IList<object> multipleMatch = new List<object> { new object(), "Match1", new object(), "Match2" };
            object errorArgument = new object();
            Action<object> errorAction = argument =>
            {
                Assert.Equal(errorArgument, argument);
                throw new InvalidOperationException();
            };

            Assert.Throws<InvalidOperationException>(() => multipleMatch.SingleOfTypeDefaultOrError<object, string, object>(errorAction, errorArgument));
        }
    }
}
