using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Tests.Helpers.Stubs;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Infrastructure;

public class DistributedLockGraphQLExtensionsTests
{
    [Fact]
    public async Task AcquireForGraphQLAsync_WhenFree_ReturnsHandle()
    {
        var distributedLock = new TestDistributedLock();

        await using var handle = await distributedLock.AcquireForGraphQLAsync("Cart:user-1", CancellationToken.None);

        handle.Resource.Should().Be("Cart:user-1");
    }

    [Fact]
    public async Task AcquireForGraphQLAsync_WhenBusy_ThrowsLockErrorWithTimeoutAsInnerException()
    {
        var distributedLock = new TestDistributedLock { IsBusy = true };

        var act = () => distributedLock.AcquireForGraphQLAsync("Cart:user-1", CancellationToken.None);

        var error = (await act.Should().ThrowAsync<LockError>()).Which;
        error.Code.Should().Be(Constants.LockedCode);
        error.InnerException.Should().BeOfType<DistributedLockTimeoutException>();
    }
}
