using System;
using System.Threading.Tasks;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    public interface IDistributedLockService
    {
        T Execute<T>(string resourceKey, Func<T> resolver);
        Task<T> ExecuteAsync<T>(string resourceKey, Func<Task<T>> resolver);
    }
}
