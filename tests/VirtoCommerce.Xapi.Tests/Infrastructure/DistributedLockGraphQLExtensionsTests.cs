using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Tests.Helpers.Stubs;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Infrastructure;

public class DistributedLockGraphQLExtensionsTests
{
    [Fact]
    public async Task AcquireForGraphQLAsync_WhenFree_ReturnsHandle()
    {
        var distributedLock = new TestDistributedLock();

        await using var handle = await distributedLock.AcquireForGraphQLAsync("Cart:user-1", TimeSpan.FromSeconds(10), CancellationToken.None);

        handle.Resource.Should().Be("Cart:user-1");
        distributedLock.Timeouts.Should().ContainSingle().Which.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task AcquireForGraphQLAsync_WhenBusy_ThrowsLockErrorWithTimeoutAsInnerException()
    {
        var distributedLock = new TestDistributedLock { IsBusy = true };

        var act = () => distributedLock.AcquireForGraphQLAsync("Cart:user-1", TimeSpan.FromSeconds(10), CancellationToken.None);

        var error = (await act.Should().ThrowAsync<LockError>()).Which;
        error.Code.Should().Be(Constants.LockedCode);
        error.InnerException.Should().BeOfType<DistributedLockTimeoutException>();
    }

    [Fact]
    public async Task AcquireForGraphQLAsync_WithContext_UsesConfiguredTimeoutAndRequestToken()
    {
        var distributedLock = new TestDistributedLock();
        using var cancellation = new CancellationTokenSource();
        using var requestServices = new ServiceCollection()
            .AddSingleton(Options.Create(new GraphQLDistributedLockOptions { Timeout = TimeSpan.FromSeconds(4) }))
            .BuildServiceProvider();
        var context = new ResolveFieldContext { RequestServices = requestServices, CancellationToken = cancellation.Token };

        await using var handle = await distributedLock.AcquireForGraphQLAsync("Cart:user-1", context);

        distributedLock.Timeouts.Should().ContainSingle().Which.Should().Be(TimeSpan.FromSeconds(4));
        distributedLock.Tokens.Should().ContainSingle().Which.Should().Be(cancellation.Token);
    }

    [Fact]
    public async Task AcquireForGraphQLAsync_WithContextWithoutOptions_WaitsTenSeconds()
    {
        var distributedLock = new TestDistributedLock();

        await using var handle = await distributedLock.AcquireForGraphQLAsync("Cart:user-1", new ResolveFieldContext());

        distributedLock.Timeouts.Should().ContainSingle().Which.Should().Be(TimeSpan.FromSeconds(10));
    }
}
