namespace CommandProcessing.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Transactions;

    /// <summary>
    /// Represents a filter for encapsulate a transaction on handlers.
    /// </summary>
    public class TransactionFilter : FilterAttribute, IHandlerFilter
    {
        private const string Key = "__TransactionFilterKey";

        public TransactionFilter()
        {
            this.ScopeOption = TransactionScopeOption.Required;
            this.Timeout = TransactionManager.DefaultTimeout;
            this.IsolationLevel = IsolationLevel.Serializable;
        }

        public TransactionScopeOption ScopeOption { get; set; }

        public TimeSpan Timeout { get; set; }

        public IsolationLevel IsolationLevel { get; set; }

        public void OnCommandExecuting(HandlerExecutingContext context)
        {
            var stack = GetStack(context.CommandContext);
            if (stack == null)
            {
                stack = new Stack<TransactionScope>();
                context.CommandContext.Items[Key] = stack;
            }

            var options = new TransactionOptions { Timeout = this.Timeout, IsolationLevel = this.IsolationLevel };
            var transactionScope = new TransactionScope(this.ScopeOption, options);
            stack.Push(transactionScope);
        }

        public void OnCommandExecuted(HandlerExecutedContext context)
        {
            var stack = GetStack(context.Context.CommandContext);
            if (stack != null && stack.Count > 0)
            {
                using (var scope = stack.Pop())
                {
                    if (null != scope && (context.Exception == null || context.ExceptionHandled))
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
