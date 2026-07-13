using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using VirtoCommerce.Xapi.Core.Services;

namespace VirtoCommerce.Xapi.Data.Services;

public class RequestScopedCache : IRequestScopedCache
{
    // Lazy<Task<object>> -> the factory runs at most once per key even under a concurrent same-key miss.
    private readonly ConcurrentDictionary<string, Lazy<Task<object>>> _cache = new();

    public virtual async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory)
    {
        var lazy = _cache.GetOrAdd(key, _ => new Lazy<Task<object>>(async () => await factory()));
        return (T)await lazy.Value;
    }
}
