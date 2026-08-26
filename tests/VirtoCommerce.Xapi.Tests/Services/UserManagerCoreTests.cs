using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Xapi.Core.Security.Authorization;
using VirtoCommerce.Xapi.Core.Services;
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
        public async Task CheckUserState_WhenTheOverrideThrowsAnotherExecutionError_ThrowsItsOwnErrorPerCall()
        {
            var sut = CreateSut();
            sut.RefuseWithCustomError = true;

            // ValidateUserStateAsync is a published seam, so an override may raise any ExecutionError - and
            // GraphQL.NET stamps Path and Locations onto every one of them as-is, not only onto ours.
            var first = await Assert.ThrowsAnyAsync<ExecutionError>(() => sut.CheckUserStateAsync(UserId));
            var second = await Assert.ThrowsAnyAsync<ExecutionError>(() => sut.CheckUserStateAsync(UserId));

            second.Should().NotBeSameAs(first);
            second.Message.Should().Be(first.Message);
            second.Code.Should().Be(first.Code);
            sut.Validations.Should().Be(1);

            // The accepted loss: a subclass is rebuilt as a plain ExecutionError, for the first caller as well
            // as the rest, so no field reports a richer error than its siblings.
            first.Should().BeOfType<ExecutionError>();
            second.Should().BeOfType<ExecutionError>();
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

        [Fact]
        public async Task ServiceCollection_GenericRegistration_SelectsTheCachingConstructor()
        {
            // The obsolete constructor's parameter set is a strict subset of the caching one's, so resolution
            // is deterministic and needs no explicit factory - but nothing else pins it, and falling back to
            // the obsolete one would drop memoization with no diagnostic at all.
            var userManagerCalls = 0;

            var accessor = new Mock<IRequestScopedCacheAccessor>();
            accessor.SetupGet(x => x.Cache).Returns(new RequestScopedCache());

            var services = new ServiceCollection();
            services.AddSingleton<Func<UserManager<ApplicationUser>>>(() =>
            {
                userManagerCalls++;

                return CreateUserManager();
            });
            services.AddSingleton(accessor.Object);
            services.AddTransient<IUserManagerCore, UserManagerCore>();

            using var provider = services.BuildServiceProvider(validateScopes: true);
            var sut = provider.GetRequiredService<IUserManagerCore>();

#pragma warning disable VC0009 // the id-taking entry point, so the check runs without a GraphQL context
            await sut.CheckUserState(UserId, allowAnonymous: true);
            await sut.CheckUserState(UserId, allowAnonymous: true);
#pragma warning restore VC0009

            // One UserManager built for two calls: the resolved instance memoizes, so DI handed it the accessor.
            userManagerCalls.Should().Be(1);
        }

        // A UserManager over a store that finds nobody: with allowAnonymous the check passes without touching
        // anything else, so the test measures how often one is built rather than what it decides.
        private static UserManager<ApplicationUser> CreateUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            store.Setup(x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((ApplicationUser)null);

            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(x => x.Value).Returns(new IdentityOptions());

            return new UserManager<ApplicationUser>(store.Object, options.Object, null, null, null, null, null, null, null);
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

            public bool RefuseWithCustomError { get; set; }

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

                if (RefuseWithCustomError)
                {
                    throw new CustomExecutionError();
                }
            }

            // Any ExecutionError an override might raise instead of ours.
            private sealed class CustomExecutionError : ExecutionError
            {
                public CustomExecutionError()
                    : base("Refused by the override")
                {
                    Code = "custom-refusal";
                }
            }
        }
    }
}
