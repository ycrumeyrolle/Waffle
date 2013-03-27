namespace CommandProcessing.Tests.Filters
{
    using System;
    using System.Threading;
    using System.Transactions;
    using CommandProcessing.Filters;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TransactionttributeFixture
    {
        [TestMethod]
        public void WhenCreatingInstanceThenPropertiesAreDefined()
        {
            // Arrange & Act
            TransactionFilter filter = new TransactionFilter();

            // Assert
            Assert.AreEqual(TransactionScopeOption.Required, filter.ScopeOption);
            Assert.AreEqual(TransactionManager.DefaultTimeout, filter.Timeout);
            Assert.AreEqual(IsolationLevel.Serializable, filter.IsolationLevel);
        }

        [TestMethod]
        public void WhenInvokingFilterWithoutExceptionThenTransactionCompletes()
        {
            // Arrange
            TransactionFilter filter = new TransactionFilter();
            HandlerExecutingContext executingContext = new HandlerExecutingContext(new HandlerContext());
            HandlerExecutedContext executedContext = new HandlerExecutedContext(executingContext, false, null);

            // Act
            filter.OnCommandExecuting(executingContext);
            Transaction transaction = Transaction.Current.Clone();
            filter.OnCommandExecuted(executedContext);

            // Assert 
            Assert.AreEqual(TransactionStatus.Committed, transaction.TransactionInformation.Status);
        }

        [TestMethod]
        public void WhenInvokingFilterWithExceptionThenTransactionRollbacks()
        {
            // Arrange
            TransactionFilter filter = new TransactionFilter();
            HandlerExecutingContext executingContext = new HandlerExecutingContext(new HandlerContext());
            HandlerExecutedContext executedContext = new HandlerExecutedContext(executingContext, false, new Exception());

            // Act
            filter.OnCommandExecuting(executingContext);
            Transaction transaction = Transaction.Current.Clone();
            filter.OnCommandExecuted(executedContext);

            // Assert
            Assert.AreEqual(TransactionStatus.Aborted, transaction.TransactionInformation.Status);
        }
        
        [TestMethod]
        public void WhenInvokingFilterWithHandledExceptionThenTransactionCompletes()
        {
            // Arrange
            TransactionFilter filter = new TransactionFilter();
            HandlerExecutingContext executingContext = new HandlerExecutingContext(new HandlerContext());
            HandlerExecutedContext executedContext = new HandlerExecutedContext(executingContext, false, new Exception());
            executedContext.ExceptionHandled = true;

            // Act
            filter.OnCommandExecuting(executingContext);
            Transaction transaction = Transaction.Current.Clone(); 
            filter.OnCommandExecuted(executedContext);

            // Assert 
            Assert.AreEqual(TransactionStatus.Committed, transaction.TransactionInformation.Status);
        }
    }
}