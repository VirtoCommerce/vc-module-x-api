using System;
using System.Threading.Tasks;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    /// <summary>
    /// Runs a resolver under a distributed lock and reports a busy resource as <see cref="LockError"/>.
    /// </summary>
    /// <remarks>
    /// Deprecated: resolve <c>VirtoCommerce.Platform.Core.DistributedLock.IDistributedLock</c> and use
    /// <see cref="DistributedLockGraphQLExtensions.AcquireForGraphQLAsync"/>, or the
    /// <c>ResolveSynchronizedAsync</c> overload that takes <c>IDistributedLock</c>.
    /// </remarks>
    public interface IDistributedLockService
    {
        T Execute<T>(string resourceKey, Func<T> resolver);
        Task<T> ExecuteAsync<T>(string resourceKey, Func<Task<T>> resolver);
    }
}
