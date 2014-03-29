namespace Waffle.Tests.Filters
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Transactions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Waffle.Filters;

    [TestClass]
    public class TransactionttributeFixture : IDisposable
    {
        private ProcessorConfiguration config;

        public TransactionttributeFixture()
        {
            this.config = new ProcessorConfiguration();
        }

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
            HandlerRequest request = new HandlerRequest(this.config, null);
            CommandHandlerContext executingContext = new CommandHandlerContext();

            executingContext.SetResponse("OK");
            CommandHandlerExecutedContext executedContext = new CommandHandlerExecutedContext(executingContext, null);

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
            CommandHandlerContext executingContext = new CommandHandlerContext();
            Exception exception = new Exception();
            ExceptionDispatchInfo exceptionInfo = ExceptionDispatchInfo.Capture(exception);
            CommandHandlerExecutedContext executedContext = new CommandHandlerExecutedContext(executingContext, exceptionInfo);

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
            CommandHandlerContext executingContext = new CommandHandlerContext();
            Exception exception = new Exception();
            ExceptionDispatchInfo exceptionInfo = ExceptionDispatchInfo.Capture(exception);
            CommandHandlerExecutedContext executedContext = new CommandHandlerExecutedContext(executingContext, exceptionInfo);
            executingContext.SetResponse("Exception handled");

            // Act
            filter.OnCommandExecuting(executingContext);
            Transaction transaction = Transaction.Current.Clone();
            filter.OnCommandExecuted(executedContext);

            // Assert 
            Assert.AreEqual(TransactionStatus.Committed, transaction.TransactionInformation.Status);
        }

        public void Dispose()
        {
            if (this.config != null)
            {
                this.config.Dispose();
            }
        }
    }
}