using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Xapi.Web.Extensions
{
    public static class HttpContextExtensions
    {
        public static async Task<GraphQLUserContext> BuildGraphQLUserContextAsync(this HttpContext context)
        {
            var loginOnBehalfContext = new LoginOnBehalfContext
            {
                Principal = context.User,
                OperatorUserName = default
            };

            // Impersonate a user based on their VC account object id by passing that value along with the header VirtoCommerce-User-Name.
            if (loginOnBehalfContext.Principal != null)
            {
                if (!TryResolveTokenLoginOnBehalf(loginOnBehalfContext))
                {
                    await TryResolveLegacyLoginOnBehalfAsync(context, loginOnBehalfContext);
                }
            }

            var userContext = new GraphQLUserContext(loginOnBehalfContext.Principal);

            if (!string.IsNullOrEmpty(loginOnBehalfContext.OperatorUserName))
            {
                userContext.TryAdd("OperatorUserName", loginOnBehalfContext.OperatorUserName);
            }

            return userContext;
        }

        private static bool TryResolveTokenLoginOnBehalf(LoginOnBehalfContext loginOnBehalfContext)
        {
            loginOnBehalfContext.OperatorUserName = loginOnBehalfContext.Principal.FindFirstValue(PlatformConstants.Security.Claims.OperatorUserName);

            return !string.IsNullOrEmpty(loginOnBehalfContext.OperatorUserName);
        }

        private static async Task<bool> TryResolveLegacyLoginOnBehalfAsync(HttpContext context, LoginOnBehalfContext loginOnBehalfContext)
        {
            if (!context.Request.Headers.TryGetValue("VirtoCommerce-User-Name", out var userNameFromHeader) ||
                !loginOnBehalfContext.Principal.IsInRole(PlatformConstants.Security.SystemRoles.Administrator))
            {
                return false;
            }

            if (userNameFromHeader == "Anonymous")
            {
                var identity = new ClaimsIdentity();

                if (context.Request.Headers.TryGetValue("VirtoCommerce-Anonymous-User-Id", out var anonymousUserId))
                {
                    identity.AddClaim(ClaimTypes.NameIdentifier, anonymousUserId);
                }

                loginOnBehalfContext.Principal = new ClaimsPrincipal(identity);
            }
            else
            {
                var factory = context.RequestServices.GetService<Func<SignInManager<ApplicationUser>>>();
                var signInManager = factory();
                var user = await signInManager.UserManager.FindByNameAsync(userNameFromHeader);
                if (user != null)
                {
                    loginOnBehalfContext.Principal = await signInManager.CreateUserPrincipalAsync(user);
                }

                // try to find LoginOnBehalf operator, if VirtoCommerce-Operator-User-Name header is present
                if (context.Request.Headers.TryGetValue("VirtoCommerce-Operator-User-Name", out var operatorUserNameHeader))
                {
                    loginOnBehalfContext.OperatorUserName = operatorUserNameHeader;
                }
            }

            return true;
        }

        private class LoginOnBehalfContext
        {
            public ClaimsPrincipal Principal { get; set; }
            public string OperatorUserName { get; set; }
        }
    }
}
