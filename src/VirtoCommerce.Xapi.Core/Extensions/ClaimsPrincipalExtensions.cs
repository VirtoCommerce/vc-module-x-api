using System.Security.Claims;
using VirtoCommerce.Platform.Core.Security;
using static VirtoCommerce.Xapi.Core.ModuleConstants;

namespace VirtoCommerce.Xapi.Core.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetCurrentUserId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal?.GetUserId() ?? AnonymousUser.UserName;
    }
}
