using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Xapi.Data.Services;

public class InMemoryLockService : IDistributedLockService
{
    // Match the Redis implementation's bounded acquisition wait so the failure mode
    // is the same regardless of which lock backend is bound at runtime.
    private static readonly TimeSpan _acquireTimeout = TimeSpan.FromSeconds(10);

    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public T Execute<T>(string resourceKey, Func<T> resolver)
    {
        ArgumentNullException.ThrowIfNull(resourceKey);
        ArgumentNullException.ThrowIfNull(resolver);

        var sem = _locks.GetOrAdd(resourceKey, _ => new SemaphoreSlim(1, 1));
        if (!sem.Wait(_acquireTimeout))
        {
            throw new LockError("Service is busy.");
        }

        try
        {
            return resolver();
        }
        finally
        {
            sem.Release();
        }
    }

    public async Task<T> ExecuteAsync<T>(string resourceKey, Func<Task<T>> resolver)
    {
        ArgumentNullException.ThrowIfNull(resourceKey);
        ArgumentNullException.ThrowIfNull(resolver);

        var sem = _locks.GetOrAdd(resourceKey, _ => new SemaphoreSlim(1, 1));
        if (!await sem.WaitAsync(_acquireTimeout))
        {
            throw new LockError("Service is busy.");
        }

        try
        {
            return await resolver();
        }
        finally
        {
            sem.Release();
        }
    }
}
