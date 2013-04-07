namespace CommandProcessing.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Transactions;
    using CommandProcessing.Internal;

    /// <summary>
    /// Represents a filter to make handlers transactional.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class TransactionFilterAttribute : HandlerFilterAttribute
    {
        private const string Key = "__TransactionFilterKey";

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionFilterAttribute"/> class.
        /// </summary>
        public TransactionFilterAttribute()
        {
            this.ScopeOption = TransactionScopeOption.Required;
            this.Timeout = TransactionManager.DefaultTimeout;
            this.IsolationLevel = IsolationLevel.Serializable;
        }

        /// <summary>
        /// Gets or sets the <see cref="TransactionScopeOption"/> for creating the transaction scope.
        /// </summary>
        /// <value>The <see cref="TransactionScopeOption"/> for creating the transaction scope.</value>
        public TransactionScopeOption ScopeOption { get; set; }

        /// <summary>
        ///  Gets or sets the timeout period for the transaction.
        /// </summary>
        /// <value>A <see cref="System.TimeSpan"/> value that specifies the timeout period for the transaction.</value>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets or sets the isolation level of the transaction.
        /// </summary>
        /// <value>A <see cref="System.Transactions.IsolationLevel"/> enumeration that specifies the isolation level of the transaction.</value>
        public IsolationLevel IsolationLevel { get; set; }

        /// <summary>
        /// Occurs before the handle method is invoked.
        /// </summary>
        /// <param name="handlerContext">The handler context.</param>
        public override void OnCommandExecuting(HandlerContext handlerContext)
        {
            if (handlerContext == null)
            {
                throw Error.ArgumentNull("handlerContext");
            }

            Stack<TransactionScope> stack = GetStack(handlerContext);
            if (stack == null)
            {
                stack = new Stack<TransactionScope>();
                handlerContext.Items[Key] = stack;
            }

            TransactionOptions options = new TransactionOptions { Timeout = this.Timeout, IsolationLevel = this.IsolationLevel };
            TransactionScope transactionScope = null;
            try
            {
                transactionScope = new TransactionScope(this.ScopeOption, options);
                stack.Push(transactionScope);
                transactionScope = null;
            }
            finally
            {
                if (transactionScope != null)
                {
                    transactionScope.Dispose();
                }
            }
        }

        /// <summary>
        /// Occurs after the handle method is invoked.
        /// </summary>
        /// <param name="handlerExecutedContext">The handler executed context.</param>
        public override void OnCommandExecuted(HandlerExecutedContext handlerExecutedContext)
        {
            if (handlerExecutedContext == null)
            {
                throw Error.ArgumentNull("handlerExecutedContext");
            }

            Stack<TransactionScope> stack = GetStack(handlerExecutedContext.HandlerContext);
            if (stack != null && stack.Count > 0)
            {
                using (var scope = stack.Pop())
                {
                    if (null != scope && handlerExecutedContext.Result != null)
                    {
                        scope.Complete();
                    }
                }
            }
        }

        private static Stack<TransactionScope> GetStack(HandlerContext context)
        {
            object value;
            if (context.Items.TryGetValue(Key, out value))
            {
                return value as Stack<TransactionScope>;    
            }
            
            return null;
        }
    }
}
