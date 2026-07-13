using System;
using System.Threading.Tasks;

namespace VirtoCommerce.Xapi.Core.Services
{
    /// <summary>
    /// Request-scoped memoization for expensive loads: deduplicates a load that is re-issued with identical
    /// arguments many times within one GraphQL request. Concurrent same-key calls share a single factory invocation.
    /// </summary>
    /// <remarks>
    /// Use this instead of a DataLoader when many distinct items issue the same load (identical, or only a few
    /// distinct, argument combinations): a DataLoader dedups by its own node key (<c>LoadAsync(id)</c>), not by the
    /// load's arguments, so a load whose arguments repeat across differently-keyed nodes still fans out. This cache
    /// dedups by the load's own stable argument key instead.
    /// <br/><br/>
    /// Key rules: the key must include EVERY argument that affects the load's result (a missing one causes false
    /// cache hits), multi-value parts of the key must be sorted for order independence, and the key must carry a
    /// type/purpose-discriminating prefix so unrelated callers sharing the same scoped instance don't collide.
    /// <br/><br/>
    /// Failure semantics: a faulted load is cached for the remainder of the request and is not retried - every
    /// same-key call rethrows the original exception.
    /// <br/><br/>
    /// Warning: consumers must be resolved from the request scope (e.g. a handler dependency, or
    /// <see cref="GraphQL.IResolveFieldContext.RequestServices"/>) - never constructor-injected into a singleton
    /// graph type or schema builder.
    /// </remarks>
    public interface IRequestScopedCache
    {
        /// <summary>
        /// Returns the result of a previous (possibly still in-flight) load registered under <paramref name="key"/>,
        /// or invokes <paramref name="factory"/> and caches its task for the remainder of the request.
        /// </summary>
        /// <typeparam name="T">Result type; every caller of the same key must use the same <typeparamref name="T"/>.</typeparam>
        /// <param name="key">Stable, order-independent key built from every load-affecting argument.</param>
        /// <param name="factory">The load to execute on a cache miss; runs at most once per key per request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is null.</exception>
        /// <exception cref="InvalidCastException">The key was previously used with a different <typeparamref name="T"/>.</exception>
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory);
    }
}
