using System;
using System.Security.Claims;
using VirtoCommerce.Platform.Core.Security;
using static VirtoCommerce.Xapi.Core.ModuleConstants;

namespace VirtoCommerce.Xapi.Core.Extensions;

public static class ClaimsPrincipalExtensions
{
    [Obsolete("Use VirtoCommerce.Platform.Core.Security.ClaimsPrincipalExtensions.GetUserId extension", DiagnosticId = "VC0009", UrlFormat = "https://docs.virtocommerce.org/platform/user-guide/versions/virto3-products-versions/")]
    public static string GetCurrentUserId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal?.GetUserId() ?? AnonymousUser.UserName;
    }
}
