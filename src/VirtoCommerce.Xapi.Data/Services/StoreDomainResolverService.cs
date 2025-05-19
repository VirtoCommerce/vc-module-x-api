using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Services;

namespace VirtoCommerce.Xapi.Data.Services;

public class StoreDomainResolverService : IStoreDomainResolverService
{
    private readonly IStoreService _storeService;
    private readonly IStoreSearchService _storeSearchService;
    private readonly StoresOptions _storeOptions;

    public StoreDomainResolverService(
        IStoreService storeService,
        IStoreSearchService storeSearchService,
        IOptions<StoresOptions> storeOptions)

    {
        _storeService = storeService;
        _storeSearchService = storeSearchService;
        _storeOptions = storeOptions.Value;
    }

    public async Task<Store> GetStoreAsync(StoreDomainRequest request)
    {
        Store store = null;

        if (!string.IsNullOrEmpty(request.StoreId))
        {
            store = await _storeService.GetByIdAsync(request.StoreId, clone: false);
        }
        else if (!string.IsNullOrEmpty(request.Domain))
        {
            store = await ResolveStoreByDomain(request.Domain);

            if (store == null && !string.IsNullOrEmpty(_storeOptions.DefaultStore))
            {
                store = await _storeService.GetByIdAsync(_storeOptions.DefaultStore, clone: false);
            }
        }

        return store;
    }

    protected virtual async Task<Store> ResolveStoreByDomain(string domain)
    {
        if (_storeOptions.Domains.TryGetValue(domain, out var storeId))
        {
            return await _storeService.GetByIdAsync(storeId, clone: false);
        }

        var criteria = AbstractTypeFactory<StoreSearchCriteria>.TryCreateInstance();
        criteria.Domain = domain;
        criteria.Take = 1;

        var result = await _storeSearchService.SearchAsync(criteria, clone: false);
        return result.Results.FirstOrDefault();
    }
}
