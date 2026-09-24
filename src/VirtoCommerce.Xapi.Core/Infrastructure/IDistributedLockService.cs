using System;
using System.Threading.Tasks;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    /// <summary>
    /// Runs a resolver under a distributed lock and reports a busy resource as <see cref="LockError"/>.
    /// </summary>
    /// <remarks>
    /// Obsolete: resolve <c>VirtoCommerce.Platform.Core.DistributedLock.IDistributedLock</c> and use
    /// <c>DistributedLockGraphQLExtensions.AcquireForGraphQLAsync</c>, or the
    /// <c>ResolveSynchronizedAsync</c> overload that takes <c>IDistributedLock</c>.
    /// </remarks>
    [Obsolete(
        "Use IDistributedLock from VirtoCommerce.Platform.Core.DistributedLock with AcquireForGraphQLAsync, or the ResolveSynchronized/ResolveSynchronizedAsync overloads that take IDistributedLock.",
        DiagnosticId = "VC0015",
        UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public interface IDistributedLockService
    {
        T Execute<T>(string resourceKey, Func<T> resolver);
        Task<T> ExecuteAsync<T>(string resourceKey, Func<Task<T>> resolver);
    }
}
