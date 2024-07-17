using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.Xapi.Core.Services
{
    public interface IUserManagerCore
    {
        Task<bool> IsLockedOutAsync(ApplicationUser user);

        Task CheckUserState(string userId, bool allowAnonymous);
    }
}
