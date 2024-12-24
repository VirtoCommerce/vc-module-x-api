using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Transport;
using GraphQL.Types;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    public class GraphQLHttpMiddlewareWithLogs<TSchema> : GraphQLHttpMiddleware<TSchema>
        where TSchema : ISchema
    {
        private readonly TelemetryClient _telemetryClient;

        public GraphQLHttpMiddlewareWithLogs(
            RequestDelegate next,
            IGraphQLTextSerializer serializer,
            IDocumentExecuter<TSchema> documentExecuter,
            IServiceScopeFactory serviceScopeFactory,
            GraphQLHttpMiddlewareOptions options,
            IHostApplicationLifetime hostApplicationLifetime,
            TelemetryClient telemetryClient)
            : base(next, serializer, documentExecuter, serviceScopeFactory, options, hostApplicationLifetime)
        {
            _telemetryClient = telemetryClient;
        }

        protected override async Task<ExecutionResult> ExecuteRequestAsync(HttpContext context, GraphQLRequest request, IServiceProvider serviceProvider, IDictionary<string, object> userContext)
        {
            // process Playground schema introspection queries without AppInsights logging
            if (_telemetryClient == null || request.OperationName == "IntrospectionQuery")
            {
                return await base.ExecuteRequestAsync(context, request, serviceProvider, userContext);
            }

            // prepare AppInsights telemetry
            var appInsightsOperationName = $"POST graphql/{request.OperationName}";

            var requestTelemetry = new RequestTelemetry
            {
                Name = appInsightsOperationName,
                Url = new Uri(context.Request.GetEncodedUrl()),
            };

            //Replace W3C Trace Context id generation https://www.w3.org/TR/trace-context/ to unique value
            requestTelemetry.Context.Operation.Id = Guid.NewGuid().ToString("N");
            requestTelemetry.Context.Operation.Name = appInsightsOperationName;
            requestTelemetry.Properties["Type"] = "GraphQL";
            using var operation = _telemetryClient.StartOperation(requestTelemetry);

            // execute GraphQL query
            var result = await base.ExecuteRequestAsync(context, request, serviceProvider, userContext);
            requestTelemetry.Success = result.Errors.IsNullOrEmpty();

            if (requestTelemetry.Success == false)
            {
                // pass an error response code to trigger AppInsights operation failure state
                requestTelemetry.ResponseCode = "500";

                var exception = result.Errors.Count > 1
                    ? new AggregateException(result.Errors)
                    : result.Errors.FirstOrDefault() as Exception;

                var exceptionTelemetry = new ExceptionTelemetry(exception);

                // link exception with the operation
                exceptionTelemetry.Context.Operation.ParentId = requestTelemetry.Context.Operation.Id;
                exceptionTelemetry.Context.Operation.Name = appInsightsOperationName;

                _telemetryClient.TrackException(exceptionTelemetry);
            }

            return result;
        }
    }
}
