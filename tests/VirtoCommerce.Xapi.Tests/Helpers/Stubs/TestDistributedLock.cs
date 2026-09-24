using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.DistributedLock;

namespace VirtoCommerce.Xapi.Tests.Helpers.Stubs;

public sealed class TestDistributedLock : IDistributedLock
{
    private int _released;

    public bool IsBusy { get; set; }

    public ConcurrentQueue<string> Resources { get; } = new();

    public ConcurrentQueue<CancellationToken> Tokens { get; } = new();

    public int Released => Volatile.Read(ref _released);

    public Task<IDistributedLockHandle> AcquireAsync(string resource, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        Resources.Enqueue(resource);
        Tokens.Enqueue(cancellationToken);

        return IsBusy
            ? Task.FromException<IDistributedLockHandle>(new DistributedLockTimeoutException(resource, timeout ?? TimeSpan.FromSeconds(30)))
            : Task.FromResult<IDistributedLockHandle>(new Handle(this, resource));
    }

    public Task<IDistributedLockHandle> TryAcquireAsync(string resource, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        Resources.Enqueue(resource);
        Tokens.Enqueue(cancellationToken);

        return Task.FromResult<IDistributedLockHandle>(IsBusy ? null : new Handle(this, resource));
    }

    private sealed class Handle : IDistributedLockHandle
    {
        private readonly TestDistributedLock _owner;

        public Handle(TestDistributedLock owner, string resource)
        {
            _owner = owner;
            Resource = resource;
        }

        public string Resource { get; }

        public void Dispose()
        {
            Interlocked.Increment(ref _owner._released);
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
