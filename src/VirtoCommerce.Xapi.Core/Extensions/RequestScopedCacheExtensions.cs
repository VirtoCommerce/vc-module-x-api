using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Services;

namespace VirtoCommerce.Xapi.Core.Extensions;

public static class RequestScopedCacheExtensions
{
    /// <summary>
    /// By-id form for items keyed by <see cref="IEntity.Id"/>: delegates to
    /// <see cref="IRequestScopedCache.GetOrAddAsync{T}(string, IEnumerable{string}, Func{T, string}, Func{IReadOnlyCollection{string}, Task{IEnumerable{T}}})"/>.
    /// </summary>
    public static Task<IReadOnlyDictionary<string, T>> GetOrAddAsync<T>(
        this IRequestScopedCache cache,
        string keyPrefix,
        IEnumerable<string> ids,
        Func<IReadOnlyCollection<string>, Task<IEnumerable<T>>> loadMissing)
        where T : class, IEntity
    {
        return cache.GetOrAddAsync(keyPrefix, ids, static x => x.Id, loadMissing);
    }
}
