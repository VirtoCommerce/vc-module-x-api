using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Server.Transports.AspNetCore.WebSockets;
using GraphQL.Transport;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace VirtoCommerce.Xapi.Core.Subscriptions.Infrastructure
{
    /// <summary>
    /// Try to resolver user context for subscriptions by validating JWT token in the request's Authorization header
    /// </summary>
    public class SubscriptionsUserContextResolver(
        IGraphQLSerializer serializer,
        IOptionsMonitor<JwtBearerOptions> jwtBearerOptionsMonitor)
        : IWebSocketAuthenticationService
    {
        private readonly IGraphQLSerializer _serializer = serializer;
        private readonly IOptionsMonitor<JwtBearerOptions> _jwtBearerOptionsMonitor = jwtBearerOptionsMonitor;

        public async Task AuthenticateAsync(IWebSocketConnection connection, string subProtocol, OperationMessage operationMessage)
        {
            var payload = _serializer.ReadNode<Inputs>(operationMessage.Payload);

            if ((payload?.TryGetValue("Authorization", out var value) ?? false) && value is string valueString)
            {
                var principal = await BuildClaimsPrincipal(valueString);
                if (principal != null)
                {
                    // set user and indicate authentication was successful
                    connection.HttpContext.User = principal;
                }
            }
        }

        private async Task<ClaimsPrincipal> BuildClaimsPrincipal(string authorization)
        {
            ClaimsPrincipal principal = null;

            if (!authorization.StartsWith("Bearer ", StringComparison.Ordinal))
            {
                return principal;
            }

            try
            {
                var token = authorization[7..];
                var tokenOptions = _jwtBearerOptionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);

                var jsonWebTokenHandler = tokenOptions.TokenHandlers.OfType<JsonWebTokenHandler>().FirstOrDefault();
                if (jsonWebTokenHandler != null)
                {
                    var result = await jsonWebTokenHandler.ValidateTokenAsync(token, tokenOptions.TokenValidationParameters);
                    principal = new ClaimsPrincipal(result.ClaimsIdentity);
                }
            }
            catch
            {
                // attempting to validate an invalid JWT token will result in an exception
                // ignore it and proceed as an anonymous user or already authenticated user
            }

            return principal;
        }
    }
}
