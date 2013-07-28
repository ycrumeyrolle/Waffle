namespace Waffle.Tests.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Waffle.Internal;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Waffle.Tests.Helpers;

    [TestClass]
    public class CollectionExtensionsFixture
    {
        [TestMethod]
        public void AsArray_Array_ReturnsSameInstance()
        {
            object[] array = { new object(), new object() };
            IEnumerable<object> arrayAsEnumerable = array;
            object[] arrayAsArray = arrayAsEnumerable.AsArray();

            Assert.AreSame(array, arrayAsArray);
        }

        [TestMethod]
        public void AsArray_Enumerable_Copies()
        {
            IList<object> list = new List<object> { new object(), new object() };
            object[] listToArray = list.ToArray();
            IEnumerable<object> arrayAsEnumerable = listToArray;
            object[] listAsArray = arrayAsEnumerable.AsArray();

            EnumerableAssert.AreEqual(listAsArray, listToArray);
        }

        [TestMethod]
        public void AsCollection_Collection_ReturnsSameInstance()
        {
            Collection<object> collection = new Collection<object> { new object(), new object() };
            IEnumerable<object> collectionAsEnumerable = collection;
            Collection<object> collectionAsCollection = collectionAsEnumerable.AsCollection();

            Assert.AreSame(collection, collectionAsCollection);
        }

        [TestMethod]
        public void AsCollection_Enumerable_Copies()
        {
            IEnumerable<object> enumerable = new LinkedList<object>(new [] { new object(), new object() });

            Collection<object> enumerableAsCollection = enumerable.AsCollection();
            IEnumerable<object> collectionAsEnumerable = enumerableAsCollection;
            EnumerableAssert.AreEqual(enumerable, enumerableAsCollection);
        }

        [TestMethod]
        public void AsCollection_IList_Wraps()
        {
            IList<object> list = new List<object> { new object(), new object() };

            Collection<object> listAsCollection = list.AsCollection();
            list.Add(new object());

            EnumerableAssert.AreEqual(list, listAsCollection.ToList());
        }

        [TestMethod]
        public void AsIList_IList_ReturnsSameInstance()
        {
            List<object> list = new List<object> { new object(), new object() };
            IEnumerable<object> listAsEnumerable = list;
            IList<object> listAsIList = listAsEnumerable.AsIList();

            Assert.AreSame(list, listAsIList);
        }

        [TestMethod]
        public void AsIList_Enumerable_Copies()
        {
            LinkedList<object> enumerable = new LinkedList<object>();
            enumerable.AddLast(new object());
            enumerable.AddLast(new object());
            List<object> expected = enumerable.ToList();
            IEnumerable<object> listAsEnumerable = enumerable;
            IList<object> enumerableAsIList = listAsEnumerable.AsIList();

            EnumerableAssert.AreEqual(expected, enumerableAsIList);
            Assert.AreNotSame(expected, enumerableAsIList);
        }

        [TestMethod]
        public void AsList_List_ReturnsSameInstance()
        {
            List<object> list = new List<object> { new object(), new object() };
            IEnumerable<object> listAsEnumerable = list;
            List<object> listAsList = listAsEnumerable.AsList();

            Assert.AreSame(list, listAsList);
        }

        [TestMethod]
        public void AsList_Enumerable_Copies()
        {
            List<object> list = new List<object> { new object(), new object() };
            object[] array = list.ToArray();
            IEnumerable<object> arrayAsEnumerable = array;
            List<object> arrayAsList = arrayAsEnumerable.AsList();

            EnumerableAssert.AreEqual(list, arrayAsList);
            Assert.AreNotSame(list, arrayAsList);
            Assert.AreNotSame(array, arrayAsList);
        }

        public void AsList_ListWrapperCollection_ReturnsSameInstance()
        {
            List<object> list = new List<object> { new object(), new object() };
            ListWrapperCollection<object> listWrapper = new ListWrapperCollection<object>(list);
            IEnumerable<object> listWrapperAsEnumerable = listWrapper;
            List<object> listWrapperAsList = listWrapperAsEnumerable.AsList();

            Assert.AreSame(list, listWrapperAsList);
        }

        [TestMethod]
        public void SingleDefaultOrErrorIListEmptyReturnsNull()
        {
            IList<object> empty = new List<object>();
            object errorArgument = new object();
            Action<object> errorAction = argument =>
            {
                throw new InvalidOperationException();
            };

            Assert.IsNull(empty.SingleDefaultOrError(errorAction, errorArgument));
        }

        [TestMethod]
        public void SingleDefaultOrErrorIListSingleReturns()
        {
            IList<object> single = new List<object> { new object() };
            object errorArgument = new object();
            Action<object> errorAction = argument =>
            {
                throw new InvalidOperationException();
            };

            Assert.AreEqual(single[0], single.SingleDefaultOrError(errorAction, errorArgument));
        }

        [TestMethod]
        public void SingleDefaultOrErrorIListMultipleThrows()
        {
            IList<object> multiple = new List<object> { new object(), new object() };
            object errorArgument = new object();
            Action<object> errorAction = argument =>
            {
                Assert.AreEqual(errorArgument, argument);
                throw new InvalidOperationException();
            };

            ExceptionAssert.Throws<InvalidOperationException>(() => multiple.SingleDefaultOrError(errorAction, errorArgument));
        }

        [TestMethod]
        public void SingleOfTypeDefaultOrErrorIListNoMatchReturnsNull()
        {
            IList<object> noMatch = new List<object> { new object(), new object() };
            object errorArgument = new object();
            Action<object> errorAction = argument =>
            {
                throw new InvalidOperationException();
            };

            Assert.IsNull(noMatch.SingleOfTypeDefaultOrError<object, string, object>(errorAction, errorArgument));
        }

        [TestMethod]
        public void SingleOfTypeDefaultOrErrorIListOneMatchReturns()
        {
            IList<object> singleMatch = new List<object> { new object(), "Match", new object() };
            object errorArgument = new object();
            Action<object> errorAction = argument =>
            {
                throw new InvalidOperationException();
            };

            Assert.AreEqual("Match", singleMatch.SingleOfTypeDefaultOrError<object, string, object>(errorAction, errorArgument));
        }

        [TestMethod]
        public void SingleOfTypeDefaultOrErrorIListMultipleMatchesThrows()
        {
            IList<object> multipleMatch = new List<object> { new object(), "Match1", new object(), "Match2" };
            object errorArgument = new object();
            Action<object> errorAction = argument =>
            {
                Assert.AreEqual(errorArgument, argument);
                throw new InvalidOperationException();
            };

            ExceptionAssert.Throws<InvalidOperationException>(() => multipleMatch.SingleOfTypeDefaultOrError<object, string, object>(errorAction, errorArgument));
        }
    }
}
