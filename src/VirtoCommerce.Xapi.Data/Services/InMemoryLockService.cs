using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Xapi.Data.Services;

public class InMemoryLockService : IDistributedLockService
{
    // Match the Redis implementation's bounded acquisition wait so the failure mode
    // is the same regardless of which lock backend is bound at runtime.
    private static readonly TimeSpan _acquireTimeout = TimeSpan.FromSeconds(10);

    private readonly ConcurrentDictionary<string, LockEntry> _locks = new();

    public T Execute<T>(string resourceKey, Func<T> resolver)
    {
        ArgumentNullException.ThrowIfNull(resourceKey);
        ArgumentNullException.ThrowIfNull(resolver);

        var entry = AcquireEntry(resourceKey);
        try
        {
            if (!entry.Semaphore.Wait(_acquireTimeout))
            {
                throw new LockError("Service is busy.");
            }

            try
            {
                return resolver();
            }
            finally
            {
                entry.Semaphore.Release();
            }
        }
        finally
        {
            ReleaseEntry(resourceKey, entry);
        }
    }

    public async Task<T> ExecuteAsync<T>(string resourceKey, Func<Task<T>> resolver)
    {
        ArgumentNullException.ThrowIfNull(resourceKey);
        ArgumentNullException.ThrowIfNull(resolver);

        var entry = AcquireEntry(resourceKey);
        try
        {
            if (!await entry.Semaphore.WaitAsync(_acquireTimeout))
            {
                throw new LockError("Service is busy.");
            }

            try
            {
                return await resolver();
            }
            finally
            {
                entry.Semaphore.Release();
            }
        }
        finally
        {
            ReleaseEntry(resourceKey, entry);
        }
    }

    private LockEntry AcquireEntry(string resourceKey)
    {
        while (true)
        {
            var entry = _locks.GetOrAdd(resourceKey, static _ => new LockEntry());

            // Try to bump the reference count, but only if the entry is alive.
            // RefCount semantics:
            //   >= 0 → entry is alive; +1 to claim a use-reference.
            //   == -1 → entry has been tombstoned by a Release; do not use, retry GetOrAdd.
            var current = Volatile.Read(ref entry.RefCount);
            while (current >= 0)
            {
                if (Interlocked.CompareExchange(ref entry.RefCount, current + 1, current) == current)
                {
                    return entry;
                }
                current = Volatile.Read(ref entry.RefCount);
            }

            // Tombstoned. Yield to let the tombstoning thread complete its TryRemove,
            // then retry. The window is tiny (a single TryRemove call) so this is bounded.
            Thread.Yield();
        }
    }

    private void ReleaseEntry(string resourceKey, LockEntry entry)
    {
        if (Interlocked.Decrement(ref entry.RefCount) != 0)
        {
            return;
        }

        // We just hit zero. Try to atomically transition 0 → -1 (tombstone).
        // If anyone acquired between Decrement and CompareExchange, RefCount is no
        // longer 0 and the CAS fails — leave the entry alone.
        if (Interlocked.CompareExchange(ref entry.RefCount, -1, 0) != 0)
        {
            return;
        }

        // Tombstoned successfully. Remove from the dictionary, but only if it still
        // maps to OUR entry — defends against the case where another thread removed
        // and a NEW entry was inserted (shouldn't happen given the tombstone, but
        // KeyValuePair-based TryRemove is the safe form).
        _locks.TryRemove(new KeyValuePair<string, LockEntry>(resourceKey, entry));
    }

    /// <summary>Per-key lock entry. Lives in the dictionary as long as <c>RefCount &gt; 0</c>.</summary>
    private sealed class LockEntry
    {
        public readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Number of in-flight acquirers (waiting + holding). 0 means evictable;
        /// -1 means tombstoned (do not use, an eviction is in progress).
        /// </summary>
        public int RefCount;
    }
}
