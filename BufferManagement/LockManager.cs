using Nito.AsyncEx;
using ShitDB.Domain;

namespace ShitDB.BufferManagement;

public class LockManager
{
    private readonly Dictionary<TableDescriptor, AsyncReaderWriterLock> _locks = new();

    public AwaitableDisposable<IDisposable> StartRead(TableDescriptor table)
    {
        lock (_locks)
        {
            if (!_locks.ContainsKey(table))
                _locks[table] = new AsyncReaderWriterLock();

            return _locks[table].ReaderLockAsync(new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token);
        }
    }

    public AwaitableDisposable<IDisposable> StartWrite(TableDescriptor table)
    {
        lock (_locks)
        {
            if (!_locks.ContainsKey(table))
                _locks[table] = new AsyncReaderWriterLock();

            return _locks[table].WriterLockAsync(new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token);
        }
    }
}