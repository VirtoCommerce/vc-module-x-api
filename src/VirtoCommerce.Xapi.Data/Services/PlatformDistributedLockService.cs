using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Xapi.Core.Infrastructure;
using IDistributedLock = VirtoCommerce.Platform.Core.DistributedLock.IDistributedLock;

namespace VirtoCommerce.Xapi.Data.Services;

/// <summary>
/// XAPI <see cref="IDistributedLockService"/> on the Platform <see cref="IDistributedLock"/>:
/// Redis when configured, otherwise an in-process lock; waits <c>DistributedLock:DefaultTimeout</c>
/// and throws <see cref="LockError"/> when the resource stays busy.
/// </summary>
public class PlatformDistributedLockService : IDistributedLockService
{
    private readonly IDistributedLock _distributedLock;

    public PlatformDistributedLockService(IDistributedLock distributedLock)
    {
        _distributedLock = distributedLock;
    }

    public virtual T Execute<T>(string resourceKey, Func<T> resolver)
    {
        // Synchronous legacy entry point; the lock code awaits with ConfigureAwait(false), so blocking here cannot deadlock.
        using var handle = _distributedLock.AcquireForGraphQLAsync(resourceKey, CancellationToken.None).GetAwaiter().GetResult();

        return resolver();
    }

    public virtual async Task<T> ExecuteAsync<T>(string resourceKey, Func<Task<T>> resolver)
    {
        var handle = await _distributedLock.AcquireForGraphQLAsync(resourceKey, CancellationToken.None).ConfigureAwait(false);
        await using (handle.ConfigureAwait(false))
        {
            return await resolver().ConfigureAwait(false);
        }
    }
}
