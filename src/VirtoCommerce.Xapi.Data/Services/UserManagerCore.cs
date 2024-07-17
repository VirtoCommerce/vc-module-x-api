using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Xapi.Core.Security.Authorization;
using VirtoCommerce.Xapi.Core.Services;

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

        public async Task CheckUserState(string userId, bool allowAnonymous)
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

            if (user.PasswordExpired)
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
