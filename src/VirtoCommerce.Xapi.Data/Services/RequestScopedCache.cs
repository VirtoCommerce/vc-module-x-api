using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using VirtoCommerce.Xapi.Core.Services;

namespace VirtoCommerce.Xapi.Data.Services;

public class RequestScopedCache : IRequestScopedCache
{
    // Lazy<Task> -> the factory runs at most once per key even under a concurrent same-key miss.
    // The Task<T> itself is stored (not its unwrapped result), so value-type results are never boxed.
    private readonly ConcurrentDictionary<string, Lazy<Task>> _cache = new();

    public virtual Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var lazy = _cache.GetOrAdd(key, static (_, arg) => new Lazy<Task>(arg), factory);

        return (Task<T>)lazy.Value;
    }
}
