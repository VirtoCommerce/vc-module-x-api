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
using VirtoCommerce.Xapi.Core.Security.Authorization;

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
                activity.SetTag("http.route", $"graphql/{request.OperationName}");
                activity.SetTag("graphql.type", "GraphQL");
            }

            var result = await base.ExecuteRequestAsync(context, request, serviceProvider, userContext);

            if (!result.Errors.IsNullOrEmpty() && activity != null)
            {
                TrackErrors(activity, result.Errors);
            }

            return result;
        }

        private static void TrackErrors(Activity activity, ExecutionErrors errors)
        {
            activity.SetTag("graphql.error.codes", string.Join(",", errors.Select(x => x.Code).Where(x => !x.IsNullOrEmpty()).Distinct()));

            // an authorization outcome is not a server fault: the client gets HTTP 200 with the error code,
            // so leave the span status unset (as OTel does for 4xx on server spans) and do not record an exception
            if (errors.All(x => x is AuthorizationError))
            {
                return;
            }

            Exception exception = errors.Count > 1
                ? new AggregateException(errors)
                : errors[0];

            activity.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity.AddException(exception);
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
