using System;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Extensions;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Security.Authorization;
using VirtoCommerce.Xapi.Core.Services;
using static VirtoCommerce.Xapi.Core.ModuleConstants;

namespace VirtoCommerce.Xapi.Data.Services
{
    public class UserManagerCore : IUserManagerCore
    {
        private readonly Func<UserManager<ApplicationUser>> _userManagerFactory;

        public UserManagerCore(Func<UserManager<ApplicationUser>> userManagerFactory)
        {
            _userManagerFactory = userManagerFactory;
        }

        public virtual async Task<bool> IsLockedOutAsync(ApplicationUser user)
        {
            using var userManager = _userManagerFactory();

            var result = await userManager.IsLockedOutAsync(user);

            return result;
        }

        [Obsolete("Use CheckCurrentUserState()", DiagnosticId = "VC0009", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        public Task CheckUserState(string userId, bool allowAnonymous)
        {
            return CheckUserState(userId, allowAnonymous, isExternalSignIn: false);
        }

        public Task CheckCurrentUserState(IResolveFieldContext context, bool allowAnonymous)
        {
            var principal = context.GetCurrentPrincipal();
            var userId = principal?.GetUserId() ?? AnonymousUser.UserName;
            var isExternalSignIn = principal.IsExternalSignIn();

            return CheckUserState(userId, allowAnonymous, isExternalSignIn);
        }


        private async Task CheckUserState(string userId, bool allowAnonymous, bool isExternalSignIn)
        {
            var userManager = _userManagerFactory();
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                if (allowAnonymous)
                {
                    return;
                }

                throw AuthorizationError.AnonymousAccessDenied();
            }

            if (user.PasswordExpired && !isExternalSignIn)
            {
                throw AuthorizationError.PasswordExpired();
            }

            if (await userManager.IsLockedOutAsync(user))
            {
                throw AuthorizationError.UserLocked();
            }
        }
    }
}
