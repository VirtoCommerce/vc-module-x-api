using System;
using System.Threading.Tasks;
using GraphQL;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.Xapi.Core.Services
{
    public interface IUserManagerCore
    {
        Task<bool> IsLockedOutAsync(ApplicationUser user);

        [Obsolete("Use CheckCurrentUserState()", DiagnosticId = "VC0009", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        Task CheckUserState(string userId, bool allowAnonymous);

        Task CheckCurrentUserState(IResolveFieldContext context, bool allowAnonymous);
    }
}
