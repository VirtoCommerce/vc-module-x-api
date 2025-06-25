using System.Threading.Tasks;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Services;

public interface IStoreDomainResolverService
{
    Task<Store> GetStoreAsync(StoreDomainRequest request);
}
