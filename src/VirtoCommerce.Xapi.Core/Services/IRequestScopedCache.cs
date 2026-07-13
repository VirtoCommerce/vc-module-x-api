using System;
using System.Threading.Tasks;

namespace VirtoCommerce.Xapi.Core.Services
{
    /// <summary>
    /// Deduplicates an expensive load that is re-issued with identical arguments many times within
    /// ONE GraphQL request. A Scoped implementation shares one instance across every request/query/mutation
    /// send in the request, so the same key resolves to the same in-flight (or completed) result.
    /// <br/><br/>
    /// Use this instead of a DataLoader when many distinct items issue the same load (identical, or only a
    /// few distinct, argument combinations) - a DataLoader dedups by its own node key (<c>LoadAsync(id)</c>),
    /// not by the load's arguments, so a load whose arguments repeat across differently-keyed nodes still
    /// fans out. This cache dedups by the load's own stable argument key instead.
    /// <br/><br/>
    /// Key rules: the key must include EVERY argument that affects the load's result (missing one causes a
    /// false cache hit), multi-value parts of the key must be sorted for order independence, and the key
    /// should carry a type/purpose-discriminating prefix so unrelated callers sharing the same scoped
    /// instance don't collide.
    /// <br/><br/>
    /// Failure semantics: a faulted load is cached for the remainder of the request and is not retried -
    /// every same-key call rethrows the original exception.
    /// <br/><br/>
    /// Warning: consumers must be resolved from the request scope (e.g. a handler dependency, or
    /// <see cref="GraphQL.IResolveFieldContext.RequestServices"/>) - never constructor-injected into a
    /// singleton graph type or schema builder.
    /// </summary>
    public interface IRequestScopedCache
    {
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory);
    }
}
