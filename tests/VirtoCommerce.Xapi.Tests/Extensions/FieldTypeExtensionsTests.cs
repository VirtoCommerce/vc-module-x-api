using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using GraphQL.Builders;
using GraphQL.Execution;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Tests.Helpers.Stubs;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Extensions;

public class FieldTypeExtensionsTests
{
    [Fact]
    public async Task ResolveSynchronizedAsync_WithDistributedLock_LocksPrefixedKeyAndFlowsCancellation()
    {
        var distributedLock = new TestDistributedLock();
        using var cancellation = new CancellationTokenSource();
        var field = FieldBuilder<object, int>.Create("field", typeof(IntGraphType))
            .ResolveSynchronizedAsync("Cart", "userId", distributedLock, _ => Task.FromResult(9));

        var result = await field.FieldType.Resolver!.ResolveAsync(CreateContext("user-1", cancellation.Token));

        result.Should().Be(9);
        distributedLock.Resources.Should().ContainSingle().Which.Should().Be("Cart:user-1");
        distributedLock.Tokens.Should().ContainSingle().Which.Should().Be(cancellation.Token);
        distributedLock.Timeouts.Should().ContainSingle().Which.Should().Be(TimeSpan.FromSeconds(10), "the default applies without configured options");
        distributedLock.Released.Should().Be(1);
    }

    [Fact]
    public async Task ResolveSynchronizedAsync_WithDistributedLock_WaitsConfiguredTimeout()
    {
        var distributedLock = new TestDistributedLock();
        var field = FieldBuilder<object, int>.Create("field", typeof(IntGraphType))
            .ResolveSynchronizedAsync("Cart", "userId", distributedLock, _ => Task.FromResult(9));
        using var requestServices = new ServiceCollection()
            .AddSingleton(Options.Create(new GraphQLDistributedLockOptions { Timeout = TimeSpan.FromSeconds(3) }))
            .BuildServiceProvider();
        var context = CreateContext("user-1", CancellationToken.None);
        context.RequestServices = requestServices;

        await field.FieldType.Resolver!.ResolveAsync(context);

        distributedLock.Timeouts.Should().ContainSingle().Which.Should().Be(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task ResolveSynchronizedAsync_WithoutResourceKey_ResolvesWithoutLock()
    {
        var distributedLock = new TestDistributedLock();
        var field = FieldBuilder<object, int>.Create("field", typeof(IntGraphType))
            .ResolveSynchronizedAsync("Cart", "userId", distributedLock, _ => Task.FromResult(9));

        var result = await field.FieldType.Resolver!.ResolveAsync(CreateContext(userId: null, CancellationToken.None));

        result.Should().Be(9);
        distributedLock.Resources.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolveSynchronizedAsync_WhenBusy_ThrowsLockError()
    {
        var distributedLock = new TestDistributedLock { IsBusy = true };
        var field = FieldBuilder<object, int>.Create("field", typeof(IntGraphType))
            .ResolveSynchronizedAsync("Cart", "userId", distributedLock, _ => Task.FromResult(9));

        var act = async () => await field.FieldType.Resolver!.ResolveAsync(CreateContext("user-1", CancellationToken.None));

        await act.Should().ThrowAsync<LockError>();
    }

    private static ResolveFieldContext<object> CreateContext(string userId, CancellationToken cancellationToken)
    {
        var command = new Dictionary<string, object>();
        if (userId != null)
        {
            command["userId"] = userId;
        }

        return new ResolveFieldContext<object>
        {
            Arguments = new Dictionary<string, ArgumentValue>
            {
                ["command"] = new ArgumentValue(command, ArgumentSource.Literal),
            },
            CancellationToken = cancellationToken,
        };
    }
}
