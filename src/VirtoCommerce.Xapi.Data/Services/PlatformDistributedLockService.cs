using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;
using IDistributedLock = VirtoCommerce.Platform.Core.DistributedLock.IDistributedLock;

namespace VirtoCommerce.Xapi.Data.Services;

/// <summary>
/// XAPI <see cref="IDistributedLockService"/> on the Platform <see cref="IDistributedLock"/>:
/// Redis when configured, otherwise an in-process lock; waits <c>VirtoCommerce:GraphQLDistributedLock:Timeout</c>
/// (10 seconds by default) and throws <see cref="LockError"/> when the resource stays busy.
/// </summary>
[Obsolete("Compatibility implementation of the obsolete XAPI IDistributedLockService. Use IDistributedLock from VirtoCommerce.Platform.Core.DistributedLock.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
public class PlatformDistributedLockService : IDistributedLockService
{
    private readonly IDistributedLock _distributedLock;
    private readonly TimeSpan _timeout;

    public PlatformDistributedLockService(IDistributedLock distributedLock, IOptions<GraphQLDistributedLockOptions> options)
    {
        _distributedLock = distributedLock;
        _timeout = options.Value.Timeout;
    }

    public virtual T Execute<T>(string resourceKey, Func<T> resolver)
    {
        // Synchronous legacy entry point; the lock code awaits with ConfigureAwait(false), so blocking here cannot deadlock.
        using var handle = _distributedLock.AcquireForGraphQLAsync(resourceKey, _timeout, CancellationToken.None).GetAwaiter().GetResult();

        return resolver();
    }

    public virtual async Task<T> ExecuteAsync<T>(string resourceKey, Func<Task<T>> resolver)
    {
        var handle = await _distributedLock.AcquireForGraphQLAsync(resourceKey, _timeout, CancellationToken.None).ConfigureAwait(false);
        await using (handle.ConfigureAwait(false))
        {
            return await resolver().ConfigureAwait(false);
        }
    }
}
