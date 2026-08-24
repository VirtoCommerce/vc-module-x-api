using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Xapi.Core.Security.Authorization;
using VirtoCommerce.Xapi.Data.Services;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Services
{
    // The state check runs once per GraphQL root field, and each run constructs a UserManager through the
    // platform factory - a DI scope plus per-instrument Meter locks. These pin that a document selecting many
    // fields pays for it once, and that memoizing does not let a refused caller through on the second field.
    public class UserManagerCoreTests
    {
        private const string UserId = "user-1";

        [Fact]
        public async Task CheckUserState_WithinOneRequest_ValidatesOnce()
        {
            var sut = CreateSut(out var cache);

            await sut.CheckUserStateAsync(UserId);
            await sut.CheckUserStateAsync(UserId);
            await sut.CheckUserStateAsync(UserId);

            sut.Validations.Should().Be(1);
            cache.Should().NotBeNull();
        }

        [Fact]
        public async Task CheckUserState_ForDifferentUsers_ValidatesEach()
        {
            var sut = CreateSut(out _);

            await sut.CheckUserStateAsync(UserId);
            await sut.CheckUserStateAsync("user-2");
            await sut.CheckUserStateAsync(UserId);

            sut.Validations.Should().Be(2);
        }

        [Fact]
        public async Task CheckUserState_WhenTheAccountIsRefused_RefusesEveryCall()
        {
            var sut = CreateSut(out _);
            sut.Refuse = true;

            // A faulted check stays cached for the request, so the second field of the same document is refused
            // as well - the memoization must not turn a refusal into a single-field failure.
            await Assert.ThrowsAsync<AuthorizationError>(() => sut.CheckUserStateAsync(UserId));
            await Assert.ThrowsAsync<AuthorizationError>(() => sut.CheckUserStateAsync(UserId));

            sut.Validations.Should().Be(1);
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

        private static TestableUserManagerCore CreateSut(out IRequestScopedCache cache)
        {
            cache = new RequestScopedCache();

            var accessor = new Mock<IRequestScopedCacheAccessor>();
            accessor.SetupGet(x => x.Cache).Returns(cache);

            return new TestableUserManagerCore(accessor.Object);
        }

        private sealed class TestableUserManagerCore : UserManagerCore
        {
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

            public int Validations { get; private set; }

            public bool Refuse { get; set; }

            public Task CheckUserStateAsync(string userId) =>
                CheckUserState(userId, allowAnonymous: false, isExternalSignIn: false, isImpersonated: false);

            // Replaces the only part that touches the platform UserManager, so the tests measure how often the
            // check runs rather than what it decides.
            protected override Task ValidateUserStateAsync(string userId, bool allowAnonymous, bool isExternalSignIn, bool isImpersonated)
            {
                Validations++;

                return Refuse
                    ? Task.FromException(AuthorizationError.UserLocked())
                    : Task.CompletedTask;
            }
        }
    }
}
