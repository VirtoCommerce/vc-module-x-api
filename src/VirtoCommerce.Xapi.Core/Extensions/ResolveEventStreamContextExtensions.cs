//using System.Security.Claims;
//using GraphQL;

//namespace VirtoCommerce.Xapi.Core.Extensions
//{
//    public static class ResolveEventStreamContextExtensions
//    {
//        public static string GetCurrentUserId(this IResolveFieldContext resolveContext)
//        {
//            var claimsPrincipal = GetCurrentPrincipal(resolveContext);
//            return claimsPrincipal?.GetCurrentUserId();
//        }

//        public static ClaimsPrincipal GetCurrentPrincipal(this IResolveFieldContext resolveContext)
//        {
//            if (resolveContext.UserContext.TryGetValue("User", out var value))
//            {
//                var claimsPrincipal = value as ClaimsPrincipal;
//                return claimsPrincipal;
//            }

//            return null;
//        }
//    }
//}
