using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Transport;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    /// <summary>
    /// Relies solely on <see cref="System.Diagnostics.Activity"/> telemetry
    /// instead of direct <c>Microsoft.ApplicationInsights</c> types. Any configured
    /// OpenTelemetry exporter (Application Insights, OTLP, etc.) picks up the data automatically.
    /// </summary>
    public class GraphQLHttpMiddlewareWithLogs<TSchema> : GraphQLHttpMiddleware<TSchema>
        where TSchema : ISchema
    {
        private readonly ILogger _logger;

        public GraphQLHttpMiddlewareWithLogs(
            RequestDelegate next,
            IGraphQLTextSerializer serializer,
            IDocumentExecuter<TSchema> documentExecuter,
            IServiceScopeFactory serviceScopeFactory,
            GraphQLHttpMiddlewareOptions options,
            IHostApplicationLifetime hostApplicationLifetime,
            ILogger<GraphQLHttpMiddlewareWithLogs<TSchema>> logger)
            : base(next, serializer, documentExecuter, serviceScopeFactory, options, hostApplicationLifetime)
        {
            _logger = logger;
        }

        protected override async Task<ExecutionResult> ExecuteRequestAsync(
            HttpContext context,
            GraphQLRequest request,
            IServiceProvider serviceProvider,
            IDictionary<string, object> userContext)
        {
            // skip Playground schema introspection queries
            if (request?.OperationName == "IntrospectionQuery")
            {
                return await base.ExecuteRequestAsync(context, request, serviceProvider, userContext);
            }

            // enrich the server activity created by AspNetCore OTel instrumentation
            var activity = Activity.Current;
            if (activity != null && request?.OperationName.IsNullOrEmpty() == false)
            {
                activity.DisplayName = $"POST graphql/{request.OperationName}";
                activity.SetTag("url.path", $"graphql/{request.OperationName}");
                activity.SetTag("graphql.type", "GraphQL");
            }

            var result = await base.ExecuteRequestAsync(context, request, serviceProvider, userContext);

            if (!result.Errors.IsNullOrEmpty() && activity != null)
            {
                var exception = result.Errors.Count > 1
                    ? (Exception)new AggregateException(result.Errors)
                    : result.Errors.FirstOrDefault();

                activity.SetStatus(ActivityStatusCode.Error, exception?.Message);
                if (exception != null)
                {
                    activity.AddException(exception);
                }
            }

            return result;
        }

        public override async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await base.InvokeAsync(context);
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode is WebSocketError.ConnectionClosedPrematurely or WebSocketError.InvalidState)
            {
                // These are common and expected during client disconnects
                _logger.LogWarning(ex, "The WebSocket connection was terminated unexpectedly");
            }
        }
    }
}
