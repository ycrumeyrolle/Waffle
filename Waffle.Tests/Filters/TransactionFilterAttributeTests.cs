namespace Waffle.Tests.Filters
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Transactions;
    using Waffle.Filters;
    using Xunit;
    
    public class TransactionFilterAttributeTests : IDisposable
    {
        private readonly ProcessorConfiguration config;

        public TransactionFilterAttributeTests()
        {
            this.config = new ProcessorConfiguration();
        }

        [Fact]
        public void WhenCreatingInstanceThenPropertiesAreDefined()
        {
            // Arrange & Act
            TransactionFilterAttribute filter = new TransactionFilterAttribute();

            // Assert
            Assert.Equal(TransactionScopeOption.Required, filter.ScopeOption);
            Assert.Equal(TransactionManager.DefaultTimeout, filter.Timeout);
            Assert.Equal(IsolationLevel.Serializable, filter.IsolationLevel);
        }

        [Fact]
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
            Assert.Equal(TransactionStatus.Committed, transaction.TransactionInformation.Status);
        }

        [Fact]
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
            Assert.Equal(TransactionStatus.Aborted, transaction.TransactionInformation.Status);
        }

        [Fact]
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
            Assert.Equal(TransactionStatus.Committed, transaction.TransactionInformation.Status);
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