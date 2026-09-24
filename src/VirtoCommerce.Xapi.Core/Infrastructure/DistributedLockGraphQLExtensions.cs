using System;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Infrastructure;

public static class DistributedLockGraphQLExtensions
{
    /// <summary>
    /// Acquires <paramref name="resource"/> for a GraphQL resolver or command builder. Waits
    /// <c>VirtoCommerce:GraphQLDistributedLock:Timeout</c> (10 seconds by default, read from <see cref="IResolveFieldContext.RequestServices"/>),
    /// and the request's <see cref="IResolveFieldContext.CancellationToken"/> cancels the wait.
    /// A timeout becomes a GraphQL <see cref="LockError"/> (<c>ServiceAccessLocked</c>).
    /// </summary>
    /// <example>
    /// <code>
    /// await using var handle = await distributedLock.AcquireForGraphQLAsync($"Cart:{userId}", context);
    /// </code>
    /// </example>
    /// <param name="distributedLock">The Platform distributed lock.</param>
    /// <param name="resource">Lock name, for example <c>Cart:{userId}</c>.</param>
    /// <param name="context">The GraphQL resolve context.</param>
    public static Task<IDistributedLockHandle> AcquireForGraphQLAsync(this IDistributedLock distributedLock, string resource, IResolveFieldContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var timeout = context.RequestServices?.GetService<IOptions<GraphQLDistributedLockOptions>>()?.Value.Timeout
            ?? GraphQLDistributedLockOptions.DefaultTimeout;

        return distributedLock.AcquireForGraphQLAsync(resource, timeout, context.CancellationToken);
    }

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
        ArgumentNullException.ThrowIfNull(distributedLock);

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
