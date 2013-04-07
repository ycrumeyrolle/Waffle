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
            TransactionFilterAttribute filter = new TransactionFilterAttribute();

            // Assert
            Assert.AreEqual(TransactionScopeOption.Required, filter.ScopeOption);
            Assert.AreEqual(TransactionManager.DefaultTimeout, filter.Timeout);
            Assert.AreEqual(IsolationLevel.Serializable, filter.IsolationLevel);
        }

        [TestMethod]
        public void WhenInvokingFilterWithoutExceptionThenTransactionCompletes()
        {
            // Arrange
            TransactionFilterAttribute filter = new TransactionFilterAttribute();
            HandlerContext executingContext = new HandlerContext();
            executingContext.Result = "OK";
            HandlerExecutedContext executedContext = new HandlerExecutedContext(executingContext, null);

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
            TransactionFilterAttribute filter = new TransactionFilterAttribute();
            HandlerContext executingContext = new HandlerContext();
            HandlerExecutedContext executedContext = new HandlerExecutedContext(executingContext, new Exception());

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
            TransactionFilterAttribute filter = new TransactionFilterAttribute();
            HandlerContext executingContext = new HandlerContext();
            HandlerExecutedContext executedContext = new HandlerExecutedContext(executingContext, new Exception());
            executedContext.Result = "Exception handled";

            // Act
            filter.OnCommandExecuting(executingContext);
            Transaction transaction = Transaction.Current.Clone(); 
            filter.OnCommandExecuted(executedContext);

            // Assert 
            Assert.AreEqual(TransactionStatus.Committed, transaction.TransactionInformation.Status);
        }
    }
}