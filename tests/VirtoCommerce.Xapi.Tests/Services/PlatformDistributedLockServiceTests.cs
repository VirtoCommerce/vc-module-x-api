// PlatformDistributedLockService and the XAPI IDistributedLockService are obsolete but still ship; keep their tests until they are removed.
#pragma warning disable VC0015

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;
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
        var service = CreateService(distributedLock);

        var result = await service.ExecuteAsync("Cart:user-1", () => Task.FromResult(5));

        result.Should().Be(5);
        distributedLock.Resources.Should().ContainSingle().Which.Should().Be("Cart:user-1");
        distributedLock.Released.Should().Be(1);
    }

    [Fact]
    public void Execute_RunsResolverUnderLockAndReleases()
    {
        var distributedLock = new TestDistributedLock();
        var service = CreateService(distributedLock);

        service.Execute("Cart:user-1", () => 6).Should().Be(6);
        distributedLock.Released.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBusy_ThrowsLockError()
    {
        var service = CreateService(new TestDistributedLock { IsBusy = true });

        var act = () => service.ExecuteAsync("Cart:user-1", () => Task.FromResult(5));

        await act.Should().ThrowAsync<LockError>();
    }

    [Fact]
    public async Task ExecuteAsync_WaitsConfiguredTimeout()
    {
        var distributedLock = new TestDistributedLock();
        var service = CreateService(distributedLock, TimeSpan.FromSeconds(3));

        await service.ExecuteAsync("Cart:user-1", () => Task.FromResult(5));

        distributedLock.Timeouts.Should().ContainSingle().Which.Should().Be(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void AddDistributedLockService_RegistersPlatformAdapterWithTenSecondDefault()
    {
        var distributedLock = new TestDistributedLock();
        var services = new ServiceCollection();
        services.AddSingleton<IDistributedLock>(distributedLock);

        services.AddDistributedLockService(new ConfigurationBuilder().Build());

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IDistributedLockService>();
        service.Should().BeOfType<PlatformDistributedLockService>();
        service.Execute("Cart:user-1", () => 1);
        distributedLock.Timeouts.Should().ContainSingle().Which.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void AddDistributedLockService_BindsTimeoutFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["VirtoCommerce:GraphQLDistributedLock:Timeout"] = "00:00:05",
            })
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<IDistributedLock>(new TestDistributedLock());

        services.AddDistributedLockService(configuration);

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<GraphQLDistributedLockOptions>>().Value.Timeout.Should().Be(TimeSpan.FromSeconds(5));
    }

    private static PlatformDistributedLockService CreateService(TestDistributedLock distributedLock, TimeSpan? timeout = null)
    {
        var options = new GraphQLDistributedLockOptions();
        if (timeout.HasValue)
        {
            options.Timeout = timeout.Value;
        }

        return new PlatformDistributedLockService(distributedLock, Options.Create(options));
    }
}
