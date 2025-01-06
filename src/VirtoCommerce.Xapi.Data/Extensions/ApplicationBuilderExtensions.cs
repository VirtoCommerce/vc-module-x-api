using Microsoft.AspNetCore.Builder;
using VirtoCommerce.Xapi.Core;
using VirtoCommerce.Xapi.Data.Security;

namespace VirtoCommerce.Xapi.Data.Extensions;
public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAuthenticationFilter(this IApplicationBuilder applicationBuilder)
    {
        return applicationBuilder.UseWhen(httpContext => httpContext.Request.Path.StartsWithSegments(ModuleConstants.GraphQlPath), builder =>
        {
            builder.UseMiddleware<AuthenticationFilterMiddleware>();
        });
    }

}
