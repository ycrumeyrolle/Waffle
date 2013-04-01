namespace CommandProcessing.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Transactions;

    /// <summary>
    /// Represents a filter for encapsulate a transaction on handlers.
    /// </summary>
    public class TransactionFilter : HandlerFilterAttribute
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

        public override void OnCommandExecuting(HandlerContext context)
        {
            var stack = GetStack(context);
            if (stack == null)
            {
                stack = new Stack<TransactionScope>();
                context.Items[Key] = stack;
            }

            var options = new TransactionOptions { Timeout = this.Timeout, IsolationLevel = this.IsolationLevel };
            var transactionScope = new TransactionScope(this.ScopeOption, options);
            stack.Push(transactionScope);
        }

        public override void OnCommandExecuted(HandlerExecutedContext context)
        {
            var stack = GetStack(context.HandlerContext);
            if (stack != null && stack.Count > 0)
            {
                using (var scope = stack.Pop())
                {
                    if (null != scope && context.Result != null)
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
