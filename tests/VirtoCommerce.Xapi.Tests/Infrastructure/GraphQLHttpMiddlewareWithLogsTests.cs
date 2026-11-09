using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Transport;
using GraphQL.Types;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Security.Authorization;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Infrastructure
{
    public class GraphQLHttpMiddlewareWithLogsTests
    {
        public static TheoryData<ExecutionError[], string> AuthorizationErrors => new()
        {
            { [AuthorizationError.PasswordExpired()], "401" },
            { [AuthorizationError.AnonymousAccessDenied(), AuthorizationError.AnonymousAccessDenied()], "401" },
            { [AuthorizationError.UserLocked()], "401" },
            { [AuthorizationError.Forbidden(), AuthorizationError.PermissionRequired("order:read")], "403" },
            { [AuthorizationError.Forbidden(), AuthorizationError.AnonymousAccessDenied()], "401" },
        };

        [Theory]
        [MemberData(nameof(AuthorizationErrors))]
        public async Task ExecuteRequestAsync_OnlyAuthorizationErrors_TracksFailedRequestWithAuthCodeAndNoException(ExecutionError[] errors, string expectedResponseCode)
        {
            var (middleware, channel) = CreateMiddleware(errors);

            await middleware.ExecuteAsync();

            var request = channel.Items.OfType<RequestTelemetry>().Single();
            request.Success.Should().BeFalse();
            request.ResponseCode.Should().Be(expectedResponseCode);
            channel.Items.OfType<ExceptionTelemetry>().Should().BeEmpty();
        }

        [Fact]
        public async Task ExecuteRequestAsync_AuthorizationAndOtherErrors_TracksServerErrorAndException()
        {
            var (middleware, channel) = CreateMiddleware([AuthorizationError.AnonymousAccessDenied(), new ExecutionError("Boom")]);

            await middleware.ExecuteAsync();

            var request = channel.Items.OfType<RequestTelemetry>().Single();
            request.Success.Should().BeFalse();
            request.ResponseCode.Should().Be("500");
            channel.Items.OfType<ExceptionTelemetry>().Should().ContainSingle();
        }

        private static (TestMiddleware Middleware, CapturingChannel Channel) CreateMiddleware(ExecutionError[] errors)
        {
            var documentExecuter = new Mock<IDocumentExecuter<ISchema>>();
            documentExecuter
                .Setup(x => x.ExecuteAsync(It.IsAny<ExecutionOptions>()))
                .ReturnsAsync(() =>
                {
                    var result = new ExecutionResult { Errors = [] };
                    foreach (var error in errors)
                    {
                        result.Errors.Add(error);
                    }

                    return result;
                });

            var channel = new CapturingChannel();
            var telemetryConfiguration = new TelemetryConfiguration
            {
                TelemetryChannel = channel,
                ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000",
            };

            var middleware = new TestMiddleware(
                documentExecuter.Object,
                new TelemetryClient(telemetryConfiguration));

            return (middleware, channel);
        }

        private sealed class TestMiddleware(IDocumentExecuter<ISchema> documentExecuter, TelemetryClient telemetryClient)
            : GraphQLHttpMiddlewareWithLogs<ISchema>(
                _ => Task.CompletedTask,
                new GraphQLSerializer(),
                documentExecuter,
                Mock.Of<IServiceScopeFactory>(),
                new GraphQLHttpMiddlewareOptions(),
                Mock.Of<IHostApplicationLifetime>(),
                NullLogger<GraphQLHttpMiddlewareWithLogs<ISchema>>.Instance,
                telemetryClient)
        {
            public Task<ExecutionResult> ExecuteAsync()
            {
                var context = new DefaultHttpContext();
                context.Request.Method = HttpMethods.Post;
                context.Request.Scheme = "https";
                context.Request.Host = new HostString("localhost");
                context.Request.Path = "/graphql";

                return ExecuteRequestAsync(context, new GraphQLRequest { OperationName = "Test" }, Mock.Of<IServiceProvider>(), new Dictionary<string, object>());
            }
        }

        private sealed class CapturingChannel : ITelemetryChannel
        {
            public ConcurrentQueue<ITelemetry> Items { get; } = new();

            public bool? DeveloperMode { get; set; }

            public string EndpointAddress { get; set; }

            public void Send(ITelemetry item) => Items.Enqueue(item);

            public void Flush()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
