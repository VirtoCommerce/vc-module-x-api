using System.Threading.Tasks;
using GraphQL;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.Xapi.Core.Services
{
    public interface IUserManagerCore
    {
        Task<bool> IsLockedOutAsync(ApplicationUser user);

        Task CheckCurrentUserState(IResolveFieldContext context, bool allowAnonymous);
    }
}
