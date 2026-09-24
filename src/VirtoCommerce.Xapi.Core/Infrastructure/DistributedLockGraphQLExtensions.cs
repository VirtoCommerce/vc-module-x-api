using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.DistributedLock;

namespace VirtoCommerce.Xapi.Core.Infrastructure;

public static class DistributedLockGraphQLExtensions
{
    /// <summary>
    /// Acquires <paramref name="resource"/>, waiting up to <paramref name="timeout"/>. A timeout becomes a GraphQL
    /// <see cref="LockError"/> (<c>ServiceAccessLocked</c>), so resolvers report a busy resource to the client.
    /// </summary>
    /// <param name="distributedLock">The Platform distributed lock.</param>
    /// <param name="resource">Lock name, for example <c>Cart:{userId}</c>.</param>
    /// <param name="timeout">Maximum wait; usually <c>GraphQLDistributedLockOptions.Timeout</c> (<c>VirtoCommerce:GraphQLDistributedLock:Timeout</c>).</param>
    /// <param name="cancellationToken">Cancels the wait, typically the GraphQL request token.</param>
    public static async Task<IDistributedLockHandle> AcquireForGraphQLAsync(this IDistributedLock distributedLock, string resource, TimeSpan timeout, CancellationToken cancellationToken)
    {
        try
        {
            return await distributedLock.AcquireAsync(resource, timeout, cancellationToken).ConfigureAwait(false);
        }
        catch (DistributedLockTimeoutException exception)
        {
            throw new LockError("Service is busy.", exception);
        }
    }
}
