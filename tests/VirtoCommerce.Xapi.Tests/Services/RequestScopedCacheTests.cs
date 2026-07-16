using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Data.Services;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Services
{
    public class RequestScopedCacheTests
    {
        [Fact]
        public async Task GetOrAddAsync_SameKey_FactoryExecutedOnce()
        {
            var sut = new RequestScopedCache();
            var callCount = 0;

            Task<string> Factory()
            {
                Interlocked.Increment(ref callCount);
                return Task.FromResult("value");
            }

            var first = await sut.GetOrAddAsync("key", Factory);
            var second = await sut.GetOrAddAsync("key", Factory);

            first.Should().Be("value");
            second.Should().Be("value");
            callCount.Should().Be(1, "the second call for the same key must be served from the cached result, not re-invoke the factory");
        }

        [Fact]
        public async Task GetOrAddAsync_DifferentKeys_FactoryExecutedPerKey()
        {
            var sut = new RequestScopedCache();
            var callCount = 0;

            Task<int> Factory()
            {
                return Task.FromResult(Interlocked.Increment(ref callCount));
            }

            var first = await sut.GetOrAddAsync("key-1", Factory);
            var second = await sut.GetOrAddAsync("key-2", Factory);

            first.Should().Be(1);
            second.Should().Be(2);
            callCount.Should().Be(2, "distinct keys must not share a cached result");
        }

        [Fact]
        public async Task GetOrAddAsync_ConcurrentSameKey_FactoryExecutedExactlyOnce()
        {
            // The single-flight guarantee: a burst of concurrent callers racing on the same key must
            // all observe the same in-flight Task rather than each starting its own factory invocation.
            var sut = new RequestScopedCache();
            var callCount = 0;
            var release = new TaskCompletionSource();

            async Task<int> Factory()
            {
                Interlocked.Increment(ref callCount);
                await release.Task;
                return 42;
            }

            const int callers = 20;
            var tasks = new Task<int>[callers];
            for (var i = 0; i < callers; i++)
            {
                // Task.Run forces the callers onto distinct pool threads so they race on GetOrAdd for real,
                // instead of entering it cooperatively from one thread.
                tasks[i] = Task.Run(() => sut.GetOrAddAsync("key", Factory));
            }

            // Give every caller a chance to reach GetOrAddAsync before releasing the factory.
            await Task.Delay(20);
            release.SetResult();

            var results = await Task.WhenAll(tasks);

            callCount.Should().Be(1, "concurrent callers on the same key must share a single in-flight factory invocation");
            results.Should().OnlyContain(x => x == 42);
        }

        [Fact]
        public async Task GetOrAddAsync_DifferentResultTypesPerKey_BothResolveCorrectly()
        {
            var sut = new RequestScopedCache();

            var stringResult = await sut.GetOrAddAsync("string-key", () => Task.FromResult("text"));
            var intResult = await sut.GetOrAddAsync("int-key", () => Task.FromResult(7));

            stringResult.Should().Be("text");
            intResult.Should().Be(7);
        }

        [Fact]
        public async Task GetOrLoadByIdsAsync_LoadsAllMissingIdsInOneBatch()
        {
            var sut = new RequestScopedCache();
            var batches = new List<string[]>();

            var result = await sut.GetOrLoadByIdsAsync("prefix", ["a", "b", "c"], x => x.Id, CreateLoader(batches));

            batches.Should().ContainSingle().Which.Should().BeEquivalentTo("a", "b", "c");
            result.Keys.Should().BeEquivalentTo("a", "b", "c");
            result["a"].Id.Should().Be("a");
        }

        [Fact]
        public async Task GetOrLoadByIdsAsync_OverlappingCall_LoadsOnlyNotYetCachedIds()
        {
            var sut = new RequestScopedCache();
            var batches = new List<string[]>();
            var loadMissing = CreateLoader(batches);

            var first = await sut.GetOrLoadByIdsAsync("prefix", ["a", "b"], x => x.Id, loadMissing);
            var second = await sut.GetOrLoadByIdsAsync("prefix", ["b", "c"], x => x.Id, loadMissing);
            var third = await sut.GetOrLoadByIdsAsync("prefix", ["a", "b"], x => x.Id, loadMissing);

            batches.Should().HaveCount(2, "the second call must load only the ids the first call did not cache, and the third call must load nothing");
            batches[1].Should().BeEquivalentTo("c");
            second.Keys.Should().BeEquivalentTo("b", "c");
            second["b"].Should().BeSameAs(first["b"], "an overlapping id must be served from the cache, not re-loaded");
            third["a"].Should().BeSameAs(first["a"]);
        }

        [Fact]
        public async Task GetOrLoadByIdsAsync_NotFoundId_OmittedAndNegativelyCached()
        {
            var sut = new RequestScopedCache();
            var batches = new List<string[]>();
            // The loader never returns an item for "ghost".
            var loadMissing = CreateLoader(batches, x => x == "ghost" ? null : new Item(x));

            var first = await sut.GetOrLoadByIdsAsync("prefix", ["a", "ghost"], x => x.Id, loadMissing);
            var second = await sut.GetOrLoadByIdsAsync("prefix", ["a", "ghost"], x => x.Id, loadMissing);

            first.Keys.Should().BeEquivalentTo("a");
            second.Keys.Should().BeEquivalentTo("a");
            batches.Should().ContainSingle("a not-found id must be negatively cached for the request, not re-loaded");
        }

        [Fact]
        public async Task GetOrLoadByIdsAsync_NullEmptyAndDuplicateIds_NormalizedWithinCall()
        {
            var sut = new RequestScopedCache();
            var batches = new List<string[]>();

            var result = await sut.GetOrLoadByIdsAsync("prefix", [null, "", "a", "a", "b"], x => x.Id, CreateLoader(batches));

            batches.Should().ContainSingle().Which.Should().BeEquivalentTo("a", "b");
            result.Keys.Should().BeEquivalentTo("a", "b");
        }

        [Fact]
        public async Task GetOrLoadByIdsAsync_EmptyIds_ReturnsEmptyWithoutLoad()
        {
            var sut = new RequestScopedCache();
            var batches = new List<string[]>();

            var result = await sut.GetOrLoadByIdsAsync("prefix", Array.Empty<string>(), x => x.Id, CreateLoader(batches));

            result.Should().BeEmpty();
            batches.Should().BeEmpty();
        }

        [Fact]
        public async Task GetOrLoadByIdsAsync_FaultedLoad_CachedForTheRequestAndRethrown()
        {
            var sut = new RequestScopedCache();
            var callCount = 0;

            Task<IList<Item>> LoadMissing(ICollection<string> missingIds)
            {
                Interlocked.Increment(ref callCount);
                throw new InvalidOperationException("boom");
            }

            var firstCall = () => sut.GetOrLoadByIdsAsync("prefix", ["a", "b"], x => x.Id, LoadMissing);
            var secondCall = () => sut.GetOrLoadByIdsAsync("prefix", ["a"], x => x.Id, LoadMissing);

            await firstCall.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
            await secondCall.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
            callCount.Should().Be(1, "a faulted load must be cached for the request - same-id calls rethrow without re-loading");
        }

        [Fact]
        public async Task GetOrLoadByIdsAsync_NullLoaderResult_TreatedAsEmptyAndNegativelyCached()
        {
            var sut = new RequestScopedCache();
            var callCount = 0;

            Task<IList<Item>> LoadMissing(ICollection<string> missingIds)
            {
                Interlocked.Increment(ref callCount);
                return Task.FromResult<IList<Item>>(null);
            }

            var first = await sut.GetOrLoadByIdsAsync("prefix", ["a"], x => x.Id, LoadMissing);
            var second = await sut.GetOrLoadByIdsAsync("prefix", ["a"], x => x.Id, LoadMissing);

            first.Should().BeEmpty();
            second.Should().BeEmpty();
            callCount.Should().Be(1, "ids from a null (empty) load result are negatively cached for the request, not re-loaded");
        }

        [Fact]
        public async Task GetOrLoadByIdsAsync_InFlightLoad_SharedByLaterCallInsteadOfSecondLoad()
        {
            var sut = new RequestScopedCache();
            var release = new TaskCompletionSource();
            var callCount = 0;

            async Task<IList<Item>> LoadMissing(ICollection<string> missingIds)
            {
                Interlocked.Increment(ref callCount);
                await release.Task;
                return missingIds.Select(x => new Item(x)).ToList();
            }

            // The first call dispatches the load synchronously up to the await, then stays in flight.
            var firstTask = sut.GetOrLoadByIdsAsync("prefix", ["a", "b"], x => x.Id, LoadMissing);
            var secondTask = sut.GetOrLoadByIdsAsync("prefix", ["a", "b"], x => x.Id, LoadMissing);

            firstTask.IsCompleted.Should().BeFalse();
            secondTask.IsCompleted.Should().BeFalse();
            release.SetResult();

            var first = await firstTask;
            var second = await secondTask;

            callCount.Should().Be(1, "the second caller must await the in-flight load, not start its own");
            second["a"].Should().BeSameAs(first["a"]);
        }

        [Fact]
        public async Task GetOrLoadByIdsAsync_ConcurrentOverlappingCallers_EachIdLoadedAtMostOnce()
        {
            // Regression guard for per-caller full-batch amplification: under concurrent overlapping
            // misses, each id must be loaded by exactly one caller, so the total load across all
            // callers is the distinct union - never a caller's full missing set re-loaded.
            const int callers = 8;
            const int windowSize = 12;
            const int windowStride = 4;
            var sut = new RequestScopedCache();
            var release = new TaskCompletionSource();
            var batches = new List<string[]>();

            async Task<IList<Item>> LoadMissing(ICollection<string> missingIds)
            {
                lock (batches)
                {
                    batches.Add([.. missingIds]);
                }

                await release.Task;
                return missingIds.Select(x => new Item(x)).ToList();
            }

            var tasks = new Task<IDictionary<string, Item>>[callers];
            for (var i = 0; i < callers; i++)
            {
                var callerIds = Enumerable.Range(i * windowStride, windowSize).Select(x => $"id-{x}").ToArray();
                tasks[i] = Task.Run(() => sut.GetOrLoadByIdsAsync("prefix", callerIds, x => x.Id, LoadMissing));
            }

            // Give every caller a chance to reserve its ids before any load completes.
            await Task.Delay(20);
            release.SetResult();

            var results = await Task.WhenAll(tasks);

            var idSpace = (callers - 1) * windowStride + windowSize;
            var loadedIds = batches.SelectMany(x => x).ToList();
            loadedIds.Should().OnlyHaveUniqueItems("an id must never appear in two batches within one request");
            loadedIds.Should().BeEquivalentTo(
                Enumerable.Range(0, idSpace).Select(x => $"id-{x}"),
                "the union of all batches must be exactly the distinct union of requested ids");
            batches.Count.Should().BeLessThanOrEqualTo(callers, "each caller dispatches at most one batch");

            for (var i = 0; i < callers; i++)
            {
                results[i].Keys.Should().BeEquivalentTo(Enumerable.Range(i * windowStride, windowSize).Select(x => $"id-{x}"));
            }

            // Overlapping ids resolve to the same shared instance across callers.
            results[1]["id-4"].Should().BeSameAs(results[0]["id-4"]);
        }

        [Fact]
        public async Task GetOrLoadByIdsAsync_TupleKeys_DoNotCollideAcrossPrefixesOrWithByKeyEntries()
        {
            var sut = new RequestScopedCache();
            var batches = new List<string[]>();
            var loadMissing = CreateLoader(batches);

            // "P" + "A:B" and "P:A" + "B" would alias under naive string concatenation.
            var first = await sut.GetOrLoadByIdsAsync("P", ["A:B"], x => x.Id, loadMissing);
            var second = await sut.GetOrLoadByIdsAsync("P:A", ["B"], x => x.Id, loadMissing);
            var byKey = await sut.GetOrAddAsync("P:A:B", () => Task.FromResult("by-key value"));

            batches.Should().HaveCount(2, "entries under different prefixes must not alias");
            first["A:B"].Id.Should().Be("A:B");
            second["B"].Id.Should().Be("B");
            byKey.Should().Be("by-key value");
        }

        [Fact]
        public async Task GetOrLoadByIdsAsync_ExtraAndDuplicateLoadedItems_FirstWinsAndExtrasIgnored()
        {
            var sut = new RequestScopedCache();
            var requestedFirst = new Item("a");
            var requestedDuplicate = new Item("a");
            var batches = new List<string[]>();
            var loadMissing = CreateLoader(batches);

            var result = await sut.GetOrLoadByIdsAsync(
                "prefix",
                ["a"],
                x => x.Id,
                _ => Task.FromResult<IList<Item>>([requestedFirst, requestedDuplicate, new Item("x")]));

            result.Keys.Should().BeEquivalentTo("a");
            result["a"].Should().BeSameAs(requestedFirst, "the first loaded item for an id wins");

            // The unrequested extra item must not have been cached.
            var extra = await sut.GetOrLoadByIdsAsync("prefix", ["x"], x => x.Id, loadMissing);
            batches.Should().ContainSingle().Which.Should().BeEquivalentTo("x");
            extra["x"].Should().NotBeNull();
        }

        [Fact]
        public async Task GetOrLoadByIdsAsync_SamePrefixAndIdWithDifferentType_ThrowsInvalidCast()
        {
            var sut = new RequestScopedCache();

            await sut.GetOrLoadByIdsAsync("prefix", ["a"], x => x.Id, CreateLoader([]));

            var act = () => sut.GetOrLoadByIdsAsync<OtherItem>("prefix", ["a"], x => x.Id, _ => Task.FromResult<IList<OtherItem>>([]));

            await act.Should().ThrowAsync<InvalidCastException>();
        }

        [Fact]
        public async Task GetOrLoadByIdsAsync_InvalidArguments_Throw()
        {
            var sut = new RequestScopedCache();
            var loadMissing = CreateLoader([]);

            var emptyPrefix = () => sut.GetOrLoadByIdsAsync("", ["a"], x => x.Id, loadMissing);
            var nullIds = () => sut.GetOrLoadByIdsAsync("prefix", null, x => x.Id, loadMissing);
            var nullSelector = () => sut.GetOrLoadByIdsAsync<Item>("prefix", ["a"], null, loadMissing);
            var nullLoader = () => sut.GetOrLoadByIdsAsync<Item>("prefix", ["a"], x => x.Id, null);

            await emptyPrefix.Should().ThrowAsync<ArgumentException>();
            await nullIds.Should().ThrowAsync<ArgumentNullException>();
            await nullSelector.Should().ThrowAsync<ArgumentNullException>();
            await nullLoader.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetOrLoadByIdsAsync_EntityOverload_KeysByEntityIdAndSharesEntries()
        {
            var sut = new RequestScopedCache();
            var batches = new List<string[]>();

            Task<IList<EntityItem>> LoadMissing(ICollection<string> missingIds)
            {
                lock (batches)
                {
                    batches.Add([.. missingIds]);
                }

                return Task.FromResult<IList<EntityItem>>(missingIds.Select(x => new EntityItem { Id = x }).ToList());
            }

            var viaEntityOverload = await sut.GetOrLoadByIdsAsync("prefix", ["a", "b"], LoadMissing);
            var viaCoreOverload = await sut.GetOrLoadByIdsAsync("prefix", ["a", "b"], x => x.Id, LoadMissing);

            batches.Should().ContainSingle("the IEntity overload must delegate to the core overload and share its cache entries");
            viaEntityOverload.Keys.Should().BeEquivalentTo("a", "b");
            viaCoreOverload["a"].Should().BeSameAs(viaEntityOverload["a"]);
        }

        private static Func<ICollection<string>, Task<IList<Item>>> CreateLoader(
            List<string[]> batches,
            Func<string, Item> createItem = null)
        {
            createItem ??= x => new Item(x);

            return missingIds =>
            {
                lock (batches)
                {
                    batches.Add([.. missingIds]);
                }

                var items = missingIds.Select(createItem).Where(x => x is not null).ToList();

                return Task.FromResult<IList<Item>>(items);
            };
        }

        private sealed record Item(string Id);

        private sealed record OtherItem(string Id);

        private sealed class EntityItem : Entity
        {
        }
    }
}
