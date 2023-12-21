# What is this?
This is a minimal example to trigger [nhibernate/nhibernate-core#3355](https://github.com/nhibernate/nhibernate-core/issues/3355) using .NET 8 and NHibernate 5.4.7, the latest version when this was written.

# How do I run this?
Open `Program.cs` and change `connectionString` to point to your MS SQL Server instance.

Then use `dotnet run` to run the program.

# What does the program do?
It uses `connectionString` to connect to your database and then creates a table named by the `tableName` variable (default is `a_table`) and fills it with data.

Then it spawns several threads that uses NHibernate to read that data inside a transaction scope with a 1ms timeout.

# Example output
```
$ dotnet run
flushing
flush complete
running on thread 10
running on thread 14
running on thread 11
running on thread 12
running on thread 13
running on thread 15
running on thread 16
running on thread 17
running on thread 18
running on thread 19
running on thread 20
running on thread 21
running on thread 22
running on thread 23
running on thread 24
running on thread 25
running on thread 25
running on thread 16
running on thread 24
running on thread 13
running on thread 18
running on thread 19
running on thread 17
running on thread 22
running on thread 14
running on thread 12
running on thread 23
running on thread 10
running on thread 20
running on thread 11
running on thread 15
running on thread 21
running on thread 16
running on thread 25
running on thread 18
running on thread 17
running on thread 23
running on thread 15
running on thread 14
running on thread 11
running on thread 19
running on thread 20
running on thread 24
running on thread 22
running on thread 12
running on thread 21
running on thread 10
running on thread 16
running on thread 13
Unhandled exception. System.InvalidOperationException: Enumerator was modified
   at NHibernate.Util.SequencedHashMap.OrderedEnumerator.get_Current()
   at NHibernate.Engine.StatefulPersistenceContext.AfterTransactionCompletion()
   at NHibernate.Impl.SessionImpl.AfterTransactionCompletion(Boolean success, ITransaction tx)
   at NHibernate.Transaction.AdoNetWithSystemTransactionFactory.SystemTransactionContext.CompleteTransaction(Boolean isCommitted)
   at NHibernate.Transaction.AdoNetWithSystemTransactionFactory.SystemTransactionContext.ProcessSecondPhase(Enlistment enlistment, Nullable`1 success)
   at NHibernate.Transaction.AdoNetWithSystemTransactionFactory.SystemTransactionContext.System.Transactions.IEnlistmentNotification.Rollback(Enlistment enlistment)
   at System.Transactions.VolatileEnlistmentAborting.EnterState(InternalEnlistment enlistment)
   at System.Transactions.TransactionStateAborted.EnterState(InternalTransaction tx)
   at System.Transactions.Bucket.TimeoutTransactions()
   at System.Transactions.BucketSet.TimeoutTransactions()
   at System.Transactions.TransactionTable.ThreadTimer(Object state)
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location ---
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Threading.TimerQueueTimer.Fire(Boolean isThreadPool)
   at System.Threading.TimerQueue.FireNextTimers()
   at System.Threading.ThreadPoolWorkQueue.Dispatch()
   at System.Threading.PortableThreadPool.WorkerThread.WorkerThreadStart()
   at System.Threading.Thread.StartCallback()
```
