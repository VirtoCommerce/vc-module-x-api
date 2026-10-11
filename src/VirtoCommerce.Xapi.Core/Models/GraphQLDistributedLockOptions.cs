using System;

namespace VirtoCommerce.Xapi.Core.Models;

/// <summary>
/// Distributed lock settings for GraphQL resolvers, bound to the <c>VirtoCommerce:GraphQLDistributedLock</c> configuration section.
/// </summary>
public class GraphQLDistributedLockOptions
{
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How long a resolver waits for a busy resource before returning <c>ServiceAccessLocked</c>. Default: 10 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = DefaultTimeout;
}
