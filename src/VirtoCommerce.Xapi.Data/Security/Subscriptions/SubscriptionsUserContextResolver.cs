using System.Net.Http.Headers;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Server.Transports.AspNetCore.WebSockets;
using GraphQL.Transport;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Security.Extensions;

namespace VirtoCommerce.Xapi.Data.Security.Subscriptions
{
    /// <summary>
    /// Resolves user context for GraphQL subscriptions by authenticating
    /// the bearer token provided in the WebSocket initialization payload.
    /// </summary>
    public class SubscriptionsUserContextResolver(IGraphQLSerializer serializer)
        : IWebSocketAuthenticationService
    {
        private readonly IGraphQLSerializer _serializer = serializer;

        public async Task AuthenticateAsync(IWebSocketConnection connection, string subProtocol, OperationMessage operationMessage)
        {
            var payload = _serializer.ReadNode<Inputs>(operationMessage.Payload);
            var authorization = GetAuthorizationBearerValue(payload);

            if (authorization == null)
            {
                return;
            }

            var httpContext = connection.HttpContext;

            // Preserve the original header if any.
            var hadOriginalHeader = httpContext.Request.Headers.TryGetValue("Authorization", out var originalHeader);

            try
            {
                httpContext.Request.Headers.Authorization = new StringValues(authorization);

                var result = await httpContext.AuthenticateTokenAsync();

                if (result.Succeeded && result.Principal != null)
                {
                    httpContext.User = result.Principal;
                }
            }
            finally
            {
                if (hadOriginalHeader)
                {
                    httpContext.Request.Headers.Authorization = originalHeader;
                }
                else
                {
                    httpContext.Request.Headers.Remove("Authorization");
                }
            }
        }

        public static string GetAuthorizationBearerValue(Inputs payload)
        {
            if (payload == null)
            {
                return null;
            }

            if (payload.TryGetValue("Authorization", out var value) &&
                value is string authorization &&
                !authorization.IsNullOrEmpty() &&
                AuthenticationHeaderValue.TryParse(authorization, out var parsed) &&
                parsed.Scheme.EqualsIgnoreCase("Bearer"))
            {
                return authorization;
            }

            return null;
        }
    }
}
