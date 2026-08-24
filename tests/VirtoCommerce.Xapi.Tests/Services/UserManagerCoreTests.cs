using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Xapi.Core.Security.Authorization;
using VirtoCommerce.Xapi.Data.Services;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Services
{
    // The state check runs once per GraphQL root field, and each run constructs a UserManager through the
    // platform factory - a DI scope plus its own SecurityDbContext. These pin that a document selecting many
    // fields pays for it once, that memoizing does not let a refused caller through on the second field, and
    // that every argument the check reads takes part in the cache key.
    public class UserManagerCoreTests
    {
        private const string UserId = "user-1";

        [Fact]
        public async Task CheckUserState_WithinOneRequest_ValidatesOnce()
        {
            var sut = CreateSut();

            await sut.CheckUserStateAsync(UserId);
            await sut.CheckUserStateAsync(UserId);
            await sut.CheckUserStateAsync(UserId);

            sut.Validations.Should().Be(1);
        }

        [Fact]
        public async Task CheckUserState_ForDifferentUsers_ValidatesEach()
        {
            var sut = CreateSut();

            await sut.CheckUserStateAsync(UserId);
            await sut.CheckUserStateAsync("user-2");
            await sut.CheckUserStateAsync(UserId);

            sut.Validations.Should().Be(2);
        }

        // One test per remaining key component. Each would collapse to a single validation if that component
        // were dropped from the key - a false cache hit, which on an authorization check is a wrong answer.
        [Fact]
        public async Task CheckUserState_ForDifferentAllowAnonymous_ValidatesEach()
        {
            var sut = CreateSut();

            await sut.CheckUserStateAsync(UserId, allowAnonymous: false);
            await sut.CheckUserStateAsync(UserId, allowAnonymous: true);

            sut.Validations.Should().Be(2);
        }

        [Fact]
        public async Task CheckUserState_ForDifferentExternalSignIn_ValidatesEach()
        {
            var sut = CreateSut();

            await sut.CheckUserStateAsync(UserId, isExternalSignIn: false);
            await sut.CheckUserStateAsync(UserId, isExternalSignIn: true);

            sut.Validations.Should().Be(2);
        }

        [Fact]
        public async Task CheckUserState_ForDifferentImpersonated_ValidatesEach()
        {
            var sut = CreateSut();

            await sut.CheckUserStateAsync(UserId, isImpersonated: false);
            await sut.CheckUserStateAsync(UserId, isImpersonated: true);

            sut.Validations.Should().Be(2);
        }

        [Fact]
        public async Task CheckUserState_ConcurrentCallers_ValidatesOnce()
        {
            // GraphQL executes sibling root fields concurrently, so a same-key burst is the live case.
            var sut = CreateSut();
            sut.BlockValidation = true;

            const int callers = 20;
            var calls = new Task[callers];
            for (var i = 0; i < callers; i++)
            {
                // Task.Run forces the callers onto distinct pool threads so they race for real, instead of
                // entering the check cooperatively from one thread.
                calls[i] = Task.Run(() => sut.CheckUserStateAsync(UserId));
            }

            // Give every caller a chance to reach the check before the one validation in flight completes.
            await Task.Delay(20);
            sut.ReleaseValidation();

            await Task.WhenAll(calls);

            sut.Validations.Should().Be(1);
        }

        [Fact]
        public async Task CheckUserState_WhenTheAccountIsRefused_RefusesEveryCall()
        {
            var sut = CreateSut();
            sut.Refuse = true;

            // The refusal stays cached for the request, so the second field of the same document is refused as
            // well - memoizing must not turn a refusal into a single-field failure.
            await Assert.ThrowsAsync<AuthorizationError>(() => sut.CheckUserStateAsync(UserId));
            await Assert.ThrowsAsync<AuthorizationError>(() => sut.CheckUserStateAsync(UserId));

            sut.Validations.Should().Be(1);
        }

        [Fact]
        public async Task CheckUserState_WhenTheAccountIsRefused_ThrowsItsOwnErrorPerCall()
        {
            var sut = CreateSut();
            sut.Refuse = true;

            // GraphQL.NET stamps Path and Locations onto the ExecutionError it catches, so sibling fields
            // sharing one instance would each report the path of whichever field wrote to it last.
            var first = await Assert.ThrowsAsync<AuthorizationError>(() => sut.CheckUserStateAsync(UserId));
            var second = await Assert.ThrowsAsync<AuthorizationError>(() => sut.CheckUserStateAsync(UserId));

            second.Should().NotBeSameAs(first);
            second.Message.Should().Be(first.Message);
            second.Code.Should().Be(first.Code);
        }

        [Fact]
        public async Task CheckUserState_WithoutAnAmbientRequest_ValidatesEveryTime()
        {
            // A background job or startup has no request to bound a cache to; the check still runs, uncached.
            var sut = new TestableUserManagerCore(accessor: null);

            await sut.CheckUserStateAsync(UserId);
            await sut.CheckUserStateAsync(UserId);

            sut.Validations.Should().Be(2);
        }

        [Fact]
        public async Task CheckUserState_WithTheObsoleteConstructor_ValidatesEveryTime()
        {
#pragma warning disable VC0015 // the overload exists for callers that have not been updated yet
            var sut = new TestableUserManagerCore();
#pragma warning restore VC0015

            await sut.CheckUserStateAsync(UserId);
            await sut.CheckUserStateAsync(UserId);

            sut.Validations.Should().Be(2);
        }

        private static TestableUserManagerCore CreateSut()
        {
            // The real RequestScopedCache, not a fake: the single-flight and cached-refusal behaviour under
            // test belongs to it, and a fake would only restate what this code assumes about it.
            var accessor = new Mock<IRequestScopedCacheAccessor>();
            accessor.SetupGet(x => x.Cache).Returns(new RequestScopedCache());

            return new TestableUserManagerCore(accessor.Object);
        }

        private sealed class TestableUserManagerCore : UserManagerCore
        {
            private readonly TaskCompletionSource _validationGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private int _validations;

            public TestableUserManagerCore(IRequestScopedCacheAccessor accessor)
                : base(() => null, accessor)
            {
            }

#pragma warning disable VC0015 // exercising the obsolete overload is the point of one of the tests
            public TestableUserManagerCore()
                : base(() => null)
            {
            }
#pragma warning restore VC0015

            public int Validations => _validations;

            public bool Refuse { get; set; }

            public bool BlockValidation { get; set; }

            public void ReleaseValidation() => _validationGate.TrySetResult();

            public Task CheckUserStateAsync(string userId, bool allowAnonymous = false, bool isExternalSignIn = false, bool isImpersonated = false) =>
                CheckUserState(userId, allowAnonymous, isExternalSignIn, isImpersonated);

            // Replaces the only part that touches the platform UserManager, so the tests measure how often the
            // check runs rather than what it decides.
            protected override async Task ValidateUserStateAsync(string userId, bool allowAnonymous, bool isExternalSignIn, bool isImpersonated)
            {
                Interlocked.Increment(ref _validations);

                if (BlockValidation)
                {
                    await _validationGate.Task;
                }

                if (Refuse)
                {
                    throw AuthorizationError.UserLocked();
                }
            }
        }
    }
}
