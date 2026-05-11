using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Xapi.Web.Extensions
{
    public static class HttpContextExtensions
    {
        public static Task<GraphQLUserContext> BuildGraphQLUserContextAsync(this HttpContext context)
        {
            var userContext = new GraphQLUserContext(context.User);

            var operatorUserName = context.User.FindFirstValue(PlatformConstants.Security.Claims.OperatorUserName);
            userContext.TrySetOperatorUserName(operatorUserName);

            return Task.FromResult(userContext);
        }
    }
}
