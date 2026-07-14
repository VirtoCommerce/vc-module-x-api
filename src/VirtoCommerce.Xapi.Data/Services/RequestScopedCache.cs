using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Xapi.Core.Services;

namespace VirtoCommerce.Xapi.Data.Services;

public class RequestScopedCache : IRequestScopedCache
{
    // Lazy<Task> -> the factory runs at most once per key even under a concurrent same-key miss.
    // The Task<T> itself is stored (not its unwrapped result), so value-type results are never boxed.
    private readonly ConcurrentDictionary<string, Lazy<Task>> _cache = new();

    // By-id entries store the Task<T> directly - single-flight comes from promise reservation, so there
    // is no per-id factory whose start a Lazy could defer. The tuple key avoids the aliasing of string
    // concatenation ("P:A"+"B" vs "P"+"A:B") and can't collide with by-key entries (separate dictionary).
    private readonly ConcurrentDictionary<(string Prefix, string Id), Task> _cacheById = new();

    public virtual Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var lazy = _cache.GetOrAdd(key, static (_, arg) => new Lazy<Task>(arg), factory);

        return (Task<T>)lazy.Value;
    }

    public virtual async Task<IReadOnlyDictionary<string, T>> GetOrAddAsync<T>(
        string keyPrefix,
        IEnumerable<string> ids,
        Func<T, string> idSelector,
        Func<IReadOnlyCollection<string>, Task<IEnumerable<T>>> loadMissing)
        where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(keyPrefix);
        ArgumentNullException.ThrowIfNull(ids);
        ArgumentNullException.ThrowIfNull(idSelector);
        ArgumentNullException.ThrowIfNull(loadMissing);

        var result = new Dictionary<string, T>(ids switch
        {
            IReadOnlyCollection<string> collection => collection.Count,
            ICollection<string> collection => collection.Count,
            _ => 0,
        });

        List<KeyValuePair<string, Task<T>>> pending = null;
        Dictionary<string, TaskCompletionSource<T>> owned = null;

        try
        {
            // Single pass: each not-yet-cached id is reserved with a promise, and only the reservation
            // winner loads it - concurrent overlapping callers load each id at most once, never re-loading
            // ids they lost. No explicit dedup: a duplicate input id hits the just-added entry.
            foreach (var id in ids)
            {
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                var key = (keyPrefix, id);
                if (!_cacheById.TryGetValue(key, out var cached))
                {
                    // RunContinuationsAsynchronously: completing the promise must not run other
                    // callers' continuations inline on the publishing thread.
                    var reservation = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                    cached = _cacheById.GetOrAdd(key, static (_, arg) => arg.Task, reservation);
                    if (ReferenceEquals(cached, reservation.Task))
                    {
                        (owned ??= new Dictionary<string, TaskCompletionSource<T>>()).Add(id, reservation);
                        (pending ??= []).Add(new(id, reservation.Task));
                        continue;
                    }

                    // Lost the reservation race - the winner's task is a regular hit.
                }

                var task = (Task<T>)cached;
                if (task.IsCompletedSuccessfully)
                {
                    var value = await task;
                    if (value is not null)
                    {
                        result[id] = value;
                    }
                }
                else
                {
                    (pending ??= []).Add(new(id, task));
                }
            }

            if (owned is not null)
            {
                // Null result counts as empty, mirroring the platform's GetOrLoadByIdsAsync.
                var loaded = await loadMissing(owned.Keys) ?? [];

                foreach (var item in loaded)
                {
                    if (item is null)
                    {
                        continue;
                    }

                    var loadedId = idSelector(item);
                    if (!string.IsNullOrEmpty(loadedId) && owned.TryGetValue(loadedId, out var reservation))
                    {
                        // TrySetResult: duplicate ids from the load - first wins.
                        reservation.TrySetResult(item);
                    }
                }

                // Not-returned owned ids: negatively cache as null for the request.
                foreach (var reservation in owned.Values)
                {
                    reservation.TrySetResult(null);
                }
            }
        }
        catch (Exception ex)
        {
            // Fault every still-incomplete owned promise: a leaked promise would hang concurrent
            // awaiters, and the cached fault makes same-id calls rethrow (by-key failure semantics).
            if (owned is not null)
            {
                foreach (var reservation in owned.Values)
                {
                    reservation.TrySetException(ex);
                }
            }

            throw;
        }

        if (pending is not null)
        {
            foreach (var (id, task) in pending)
            {
                var value = await task;
                if (value is not null)
                {
                    result.TryAdd(id, value);
                }
            }
        }

        return result;
    }
}
