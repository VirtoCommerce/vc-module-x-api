using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
    }
}
