using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Data.Extensions;
using VirtoCommerce.Xapi.Data.Services;
using VirtoCommerce.Xapi.Tests.Helpers.Stubs;
using Xunit;
using IDistributedLock = VirtoCommerce.Platform.Core.DistributedLock.IDistributedLock;

namespace VirtoCommerce.Xapi.Tests.Services;

public class PlatformDistributedLockServiceTests
{
    [Fact]
    public async Task ExecuteAsync_RunsResolverUnderLockAndReleases()
    {
        var distributedLock = new TestDistributedLock();
        var service = new PlatformDistributedLockService(distributedLock);

        var result = await service.ExecuteAsync("Cart:user-1", () => Task.FromResult(5));

        result.Should().Be(5);
        distributedLock.Resources.Should().ContainSingle().Which.Should().Be("Cart:user-1");
        distributedLock.Released.Should().Be(1);
    }

    [Fact]
    public void Execute_RunsResolverUnderLockAndReleases()
    {
        var distributedLock = new TestDistributedLock();
        var service = new PlatformDistributedLockService(distributedLock);

        service.Execute("Cart:user-1", () => 6).Should().Be(6);
        distributedLock.Released.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBusy_ThrowsLockError()
    {
        var service = new PlatformDistributedLockService(new TestDistributedLock { IsBusy = true });

        var act = () => service.ExecuteAsync("Cart:user-1", () => Task.FromResult(5));

        await act.Should().ThrowAsync<LockError>();
    }

    [Fact]
    public void AddDistributedLockService_RegistersPlatformAdapter()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDistributedLock>(new TestDistributedLock());

        services.AddDistributedLockService(new ConfigurationBuilder().Build());

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IDistributedLockService>().Should().BeOfType<PlatformDistributedLockService>();
    }
}
