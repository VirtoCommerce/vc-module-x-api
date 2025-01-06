using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Data.Security;

public class AuthenticationFilterMiddleware : IMiddleware
{
    private readonly GraphQLOptions _options;
    private readonly ILogger<AuthenticationFilterMiddleware> _logger;

    public AuthenticationFilterMiddleware(IOptions<GraphQLOptions> optionsAccessor, ILogger<AuthenticationFilterMiddleware> logger)
    {
        _logger = logger;
        _options = optionsAccessor.Value;
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _logger.LogDebug("{Method} {Path} {AuthenticationType}",
            context.Request.Method, context.Request.Path, string.Join(", ", context.User.Identities.Select(x => x.AuthenticationType + " " + x.Name)));

        // Remove identities with forbidden authenticated types
        var identities = context.User.Identities
            .Where(x => !_options.ForbiddenAuthenticationTypes.Contains(x.AuthenticationType))
            .ToList();

        if (identities.Count == 0)
        {
            identities.Add(new ClaimsIdentity());
        }

        context.User = new ClaimsPrincipal(identities);

        return next.Invoke(context);
    }
}
