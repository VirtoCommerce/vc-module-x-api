using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Common;

namespace VirtoCommerce.Xapi.Core.Services
{
    public interface ILoadUserToEvalContextService
    {
        Task SetShopperDataFromMember(EvaluationContextBase evalContextBase, string customerId);
    }
}
