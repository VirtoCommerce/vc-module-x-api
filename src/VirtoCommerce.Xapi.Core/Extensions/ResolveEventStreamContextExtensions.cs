using System.Security.Claims;
using GraphQL.Server.Transports.Subscriptions.Abstractions;
using GraphQL.Subscription;

namespace VirtoCommerce.Xapi.Core.Extensions
{
    public static class ResolveEventStreamContextExtensions
    {
        public static string GetCurrentUserId(this IResolveEventStreamContext resolveContext)
        {
            var claimsPrincipal = GetCurrentPrincipal(resolveContext);
            return claimsPrincipal?.GetCurrentUserId();
        }

        public static ClaimsPrincipal GetCurrentPrincipal(this IResolveEventStreamContext resolveContext)
        {
            var messageContext = ((MessageHandlingContext)resolveContext.UserContext);

            if (messageContext.TryGetValue("User", out var value))
            {
                var claimsPrincipal = value as ClaimsPrincipal;
                return claimsPrincipal;
            }

            return null;
        }
    }
}
