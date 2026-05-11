using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Data.Services;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Services
{
    public class InMemoryLockServiceTests
    {
        // ---- Argument validation -------------------------------------------

        [Fact]
        public void Execute_NullResourceKey_Throws()
        {
            var sut = new InMemoryLockService();
            Action act = () => sut.Execute<int>(null, () => 0);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Execute_NullResolver_Throws()
        {
            var sut = new InMemoryLockService();
            Action act = () => sut.Execute<int>("k", null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task ExecuteAsync_NullResourceKey_Throws()
        {
            var sut = new InMemoryLockService();
            Func<Task> act = () => sut.ExecuteAsync<int>(null, () => Task.FromResult(0));
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task ExecuteAsync_NullResolver_Throws()
        {
            var sut = new InMemoryLockService();
            Func<Task> act = () => sut.ExecuteAsync<int>("k", null);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        // ---- Basic behavior -------------------------------------------------

        [Fact]
        public void Execute_ReturnsResolverResult()
        {
            var sut = new InMemoryLockService();
            sut.Execute("k", () => 42).Should().Be(42);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsResolverResult()
        {
            var sut = new InMemoryLockService();
            (await sut.ExecuteAsync("k", () => Task.FromResult(42))).Should().Be(42);
        }

        // ---- Mutual exclusion (cart contract) ------------------------------

        [Fact]
        public async Task ExecuteAsync_SameKey_SecondStartsAfterFirstEnds()
        {
            // The exact contract the cart resolvers depend on: when two parallel mutations
            // for the same userId arrive, the second one's critical section must not begin
            // before the first one's critical section ends. If this fails, a NoLockService-
            // like binding is in effect and concurrent cart writes will lost-update each other.
            var sut = new InMemoryLockService();
            const string key = "Cart:user-acme-store-employee-1";
            var clock = Stopwatch.StartNew();

            long firstEntered = -1, firstExited = -1, secondEntered = -1, secondExited = -1;

            async Task<int> First()
            {
                firstEntered = clock.ElapsedMilliseconds;
                await Task.Delay(150);
                firstExited = clock.ElapsedMilliseconds;
                return 1;
            }

            async Task<int> Second()
            {
                secondEntered = clock.ElapsedMilliseconds;
                await Task.Delay(50);
                secondExited = clock.ElapsedMilliseconds;
                return 2;
            }

            var t1 = sut.ExecuteAsync(key, First);
            // Yield once so the first call definitely owns the semaphore before the
            // second call arrives — otherwise the test could pass tautologically.
            await Task.Yield();
            var t2 = sut.ExecuteAsync(key, Second);
            await Task.WhenAll(t1, t2);

            firstEntered.Should().BeGreaterThanOrEqualTo(0);
            firstExited.Should().BeGreaterThanOrEqualTo(firstEntered);
            secondEntered.Should().BeGreaterThanOrEqualTo(0);
            secondExited.Should().BeGreaterThanOrEqualTo(secondEntered);

            secondEntered.Should().BeGreaterThanOrEqualTo(firstExited,
                $"second's critical section must not begin before first's ends — observed " +
                $"first [{firstEntered} → {firstExited}] ms, second [{secondEntered} → {secondExited}] ms");
        }

        [Fact]
        public async Task ExecuteAsync_SameKey_AlwaysAtMostOneInside()
        {
            var sut = new InMemoryLockService();
            const string key = "k";
            const int callers = 50;
            var inFlight = 0;
            var maxInFlight = 0;
            var maxLock = new object();
            var counter = 0;

            async Task<int> Critical()
            {
                var current = Interlocked.Increment(ref inFlight);
                lock (maxLock)
                {
                    if (current > maxInFlight)
                        maxInFlight = current;
                }
                await Task.Yield();
                Interlocked.Decrement(ref inFlight);
                return Interlocked.Increment(ref counter);
            }

            var tasks = new Task[callers];
            for (var i = 0; i < callers; i++)
            {
                tasks[i] = sut.ExecuteAsync(key, Critical);
            }
            await Task.WhenAll(tasks);

            maxInFlight.Should().Be(1);
            counter.Should().Be(callers);
        }

        [Fact]
        public async Task ExecuteAsync_DifferentKeys_DoNotSerialize()
        {
            // Different cart keys (e.g. different userIds) must not block each other.
            var sut = new InMemoryLockService();
            var bothEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var concurrent = 0;
            var sawTwo = false;

            async Task<int> Critical()
            {
                var n = Interlocked.Increment(ref concurrent);
                if (n >= 2)
                {
                    sawTwo = true;
                    bothEntered.TrySetResult();
                }
                await bothEntered.Task.WaitAsync(TimeSpan.FromSeconds(2));
                Interlocked.Decrement(ref concurrent);
                return n;
            }

            var t1 = sut.ExecuteAsync("Cart:user-A", Critical);
            var t2 = sut.ExecuteAsync("Cart:user-B", Critical);
            await Task.WhenAll(t1, t2);

            sawTwo.Should().BeTrue();
        }

        // ---- Resolver exceptions release the lock --------------------------

        [Fact]
        public async Task ExecuteAsync_ResolverThrows_LockIsStillReleased()
        {
            var sut = new InMemoryLockService();
            const string key = "k";

            await FluentActions.Invoking(() =>
                    sut.ExecuteAsync<int>(key, () => throw new InvalidOperationException("boom")))
                .Should().ThrowAsync<InvalidOperationException>();

            // Should not deadlock; if the lock leaked, this would block until the 10 s timeout
            // and surface as LockError. Bound the wait so the test fails fast either way.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var t = sut.ExecuteAsync(key, () => Task.FromResult(1));
            var winner = await Task.WhenAny(t, Task.Delay(2000, cts.Token));
            winner.Should().BeSameAs(t, "lock must have been released and the next call must succeed quickly");
            (await t).Should().Be(1);
        }

        [Fact]
        public void Execute_ResolverThrows_LockIsStillReleased()
        {
            var sut = new InMemoryLockService();
            const string key = "k";

            Action act = () => sut.Execute<int>(key, () => throw new InvalidOperationException("boom"));
            act.Should().Throw<InvalidOperationException>();

            // Should not block — if the lock leaked we'd hang until the 10 s acquisition timeout.
            sut.Execute(key, () => 1).Should().Be(1);
        }

        // ---- Bounded wait → LockError --------------------------------------

        [Fact]
        public async Task ExecuteAsync_AcquireTimeoutExceeded_ThrowsLockError()
        {
            // Hold the lock with a never-completing resolver, then attempt a second call.
            // The second call must not wait forever — it must fail with LockError after the
            // bounded acquisition wait. This mirrors the Redis implementation's behavior.
            //
            // We use reflection to override the timeout because the production constant (10 s)
            // would make the test slow; this validates the *mechanism*, not the specific value.
            var sut = new InMemoryLockService();
            const string key = "k";

            var release = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var holder = sut.ExecuteAsync(key, () => release.Task);

            // Give the holder time to acquire.
            await Task.Delay(20);

            // Race the second call against a generous bound — if the lock implementation
            // ever decided to wait without bound, this test would simply hang and CI would
            // time it out, surfacing the regression.
            var contender = sut.ExecuteAsync(key, () => Task.FromResult(0));
            var raceOutcome = await Task.WhenAny(contender, Task.Delay(TimeSpan.FromSeconds(15)));

            // Release the holder regardless of the outcome to clean up.
            release.SetResult(0);
            await holder;

            raceOutcome.Should().BeSameAs(contender,
                "the bounded acquisition wait must elapse and the contender must complete (with LockError) " +
                "instead of waiting indefinitely");

            await FluentActions.Invoking(() => contender)
                .Should().ThrowAsync<LockError>().WithMessage("Service is busy.");
        }

        // ---- Eviction / memory bound ---------------------------------------

        [Fact]
        public async Task ExecuteAsync_AfterRelease_DictionaryEvictsKey()
        {
            // After the last user releases a key, the per-key entry must be removed
            // from the internal dictionary — otherwise distinct user IDs accumulate
            // SemaphoreSlim instances forever and steady-state memory grows unbounded.
            var sut = new InMemoryLockService();

            await sut.ExecuteAsync("Cart:user-1", () => Task.FromResult(0));
            await sut.ExecuteAsync("Cart:user-2", () => Task.FromResult(0));
            await sut.ExecuteAsync("Cart:user-3", () => Task.FromResult(0));

            GetInternalLockCount(sut).Should().Be(0,
                "after every caller releases, the lock dictionary must drop every key " +
                "so memory stays proportional to in-flight, not ever-seen, acquisitions");
        }

        [Fact]
        public async Task ExecuteAsync_TwoOverlappingCallersSameKey_ShareEntryThenEvict()
        {
            // While two callers are in flight on the same key the entry is shared.
            // When the second one releases, the entry is evicted.
            var sut = new InMemoryLockService();
            const string key = "Cart:user-1";

            var release1 = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var first = sut.ExecuteAsync(key, async () =>
            {
                await release1.Task;
                return 0;
            });

            // Yield until the first caller has registered in the dictionary.
            await Task.Yield();
            GetInternalLockCount(sut).Should().Be(1, "first caller must have created an entry");

            // Start a second caller; it'll wait at the semaphore.
            var release2 = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var second = sut.ExecuteAsync(key, async () =>
            {
                await release2.Task;
                return 0;
            });
            // Brief wait so the second caller has reached AcquireEntry.
            await Task.Delay(20);
            GetInternalLockCount(sut).Should().Be(1,
                "two overlapping callers on the same key must share a single dictionary entry");

            // Let first finish; second runs through.
            release1.SetResult();
            await first;

            // Second is still in flight — entry must still exist.
            GetInternalLockCount(sut).Should().Be(1,
                "while a caller is still holding the lock, its entry must remain in the dictionary");

            release2.SetResult();
            await second;

            // Both done — evicted.
            GetInternalLockCount(sut).Should().Be(0);
        }

        [Fact]
        public async Task ExecuteAsync_RepeatedSameKey_DoesNotLeakEntries()
        {
            // 1,000 sequential acquisitions on the same key must not grow the dictionary.
            var sut = new InMemoryLockService();
            const string key = "Cart:user-1";

            for (var i = 0; i < 1000; i++)
            {
                await sut.ExecuteAsync(key, () => Task.FromResult(0));
            }

            GetInternalLockCount(sut).Should().Be(0,
                "sequential acquire/release cycles on the same key must not leak dictionary entries");
        }

        [Fact]
        public async Task ExecuteAsync_RepeatedDistinctKeys_DoesNotLeakEntries()
        {
            // Distinct user IDs are the original concern — must evict after each release.
            var sut = new InMemoryLockService();

            for (var i = 0; i < 1000; i++)
            {
                await sut.ExecuteAsync($"Cart:user-{i}", () => Task.FromResult(0));
            }

            GetInternalLockCount(sut).Should().Be(0,
                "distinct keys with no overlap must each evict on release; otherwise the dictionary " +
                "grows proportionally to ever-seen users, regressing vs the prior NoLockService footprint");
        }

        [Fact]
        public async Task ExecuteAsync_PostEvictionFreshAcquire_StillSerializes()
        {
            // The most subtle bug: if eviction races with a new acquire, two callers can end up
            // holding two DIFFERENT semaphore instances for the same logical key — reintroducing
            // exactly the lost-update bug we set out to fix. This test pounds on the same key
            // serially and concurrently and verifies max-in-flight stays at 1 throughout.
            var sut = new InMemoryLockService();
            const string key = "Cart:user-1";
            var inFlight = 0;
            var maxInFlight = 0;
            var maxLock = new object();

            async Task Loop(int iterations)
            {
                for (var i = 0; i < iterations; i++)
                {
                    await sut.ExecuteAsync(key, async () =>
                    {
                        var current = Interlocked.Increment(ref inFlight);
                        lock (maxLock)
                        {
                            if (current > maxInFlight)
                                maxInFlight = current;
                        }
                        await Task.Yield();
                        Interlocked.Decrement(ref inFlight);
                        return 0;
                    });
                }
            }

            // Mix concurrent and sequential pressure to maximize the chance of
            // hitting the "evict-while-acquire" window.
            const int loops = 8;
            const int iterations = 200;
            var workers = new Task[loops];
            for (var i = 0; i < loops; i++)
            {
                workers[i] = Task.Run(() => Loop(iterations));
            }
            await Task.WhenAll(workers);

            maxInFlight.Should().Be(1,
                "ref-counted eviction must never produce two concurrent holders of the same key — " +
                "if it does, the cart lost-update bug returns under load");
            GetInternalLockCount(sut).Should().Be(0,
                "after the storm subsides every entry must have been evicted");
        }

        [Fact]
        public async Task ExecuteAsync_DifferentKeysInFlight_AllEntriesPresent_ThenAllEvicted()
        {
            var sut = new InMemoryLockService();
            const int keys = 50;

            var holders = new TaskCompletionSource[keys];
            var tasks = new Task[keys];
            for (var i = 0; i < keys; i++)
            {
                holders[i] = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var local = i;
                tasks[i] = sut.ExecuteAsync($"Cart:user-{local}", () => holders[local].Task.ContinueWith(_ => 0));
            }

            // Brief wait for all 50 to register in the dictionary.
            await Task.Delay(50);
            GetInternalLockCount(sut).Should().Be(keys,
                "every distinct in-flight key must have its own entry while held");

            for (var i = 0; i < keys; i++)
            {
                holders[i].SetResult();
            }
            await Task.WhenAll(tasks);

            GetInternalLockCount(sut).Should().Be(0,
                "after every key releases, the dictionary must be empty");
        }

        // ---- Helpers --------------------------------------------------------

        /// <summary>
        /// Reflective peek at the private <c>_locks</c> dictionary so eviction-bound tests
        /// can assert dictionary size without exposing internal state on the public API.
        /// Tests intentionally couple to the field name; renaming it requires updating here.
        /// </summary>
        private static int GetInternalLockCount(InMemoryLockService sut)
        {
            var field = typeof(InMemoryLockService).GetField(
                "_locks",
                BindingFlags.Instance | BindingFlags.NonPublic);
            field.Should().NotBeNull("InMemoryLockService._locks must remain a private field for eviction tests");
            var dict = field!.GetValue(sut);
            dict.Should().BeAssignableTo<System.Collections.ICollection>();
            return ((System.Collections.ICollection)dict!).Count;
        }
    }
}
