using System;
using System.Transactions;

namespace Apollo.Core.Test
{
    public abstract class TestUtils
    {
        public static TransactionScope CreateTransaction()
        {
            TransactionOptions transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted;
            transactionOptions.Timeout = new TimeSpan(0, 1, 0);
            return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }
    }
}
