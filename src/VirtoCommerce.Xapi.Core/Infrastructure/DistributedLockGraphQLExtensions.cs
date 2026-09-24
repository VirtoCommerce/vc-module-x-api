using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.DistributedLock;

namespace VirtoCommerce.Xapi.Core.Infrastructure;

public static class DistributedLockGraphQLExtensions
{
    /// <summary>
    /// Acquires <paramref name="resource"/> with the Platform default timeout (<c>DistributedLock:DefaultTimeout</c>).
    /// A timeout becomes a GraphQL <see cref="LockError"/> (<c>ServiceAccessLocked</c>), so resolvers report a busy resource to the client.
    /// </summary>
    public static async Task<IDistributedLockHandle> AcquireForGraphQLAsync(this IDistributedLock distributedLock, string resource, CancellationToken cancellationToken)
    {
        try
        {
            return await distributedLock.AcquireAsync(resource, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (DistributedLockTimeoutException exception)
        {
            throw new LockError("Service is busy.", exception);
        }
    }
}
