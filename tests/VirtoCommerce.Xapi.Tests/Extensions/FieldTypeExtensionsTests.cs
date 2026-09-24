using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using GraphQL.Builders;
using GraphQL.Execution;
using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Infrastructure;
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
        distributedLock.Released.Should().Be(1);
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
