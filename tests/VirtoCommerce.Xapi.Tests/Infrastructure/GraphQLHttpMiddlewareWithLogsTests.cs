using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Transport;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VirtoCommerce.ApplicationInsights.Core.Telemetry;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Security.Authorization;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Infrastructure
{
    public class GraphQLHttpMiddlewareWithLogsTests
    {
        public static TheoryData<ExecutionError[], string, int> AuthorizationErrors => new()
        {
            { [AuthorizationError.PasswordExpired()], "PasswordExpired", 401 },
            { [AuthorizationError.AnonymousAccessDenied(), AuthorizationError.AnonymousAccessDenied()], "Unauthorized", 401 },
            { [AuthorizationError.UserLocked()], "UserLocked", 401 },
            { [AuthorizationError.Forbidden(), AuthorizationError.PermissionRequired("order:read")], "Forbidden", 403 },
            { [AuthorizationError.Forbidden(), AuthorizationError.AnonymousAccessDenied()], "Forbidden,Unauthorized", 401 },
        };

        [Theory]
        [MemberData(nameof(AuthorizationErrors))]
        public async Task ExecuteRequestAsync_OnlyAuthorizationErrors_SetsErrorStatusAndRecordsNoException(ExecutionError[] errors, string expectedCodes, int expectedResponseCode)
        {
            using var activity = StartActivity();

            await CreateMiddleware(errors).ExecuteAsync();

            activity.Status.Should().Be(ActivityStatusCode.Error);
            activity.Events.Should().NotContain(x => x.Name == "exception");
            activity.GetTagItem("graphql.error.codes").Should().Be(expectedCodes);
            activity.GetTagItem(TelemetryTags.ResponseStatusCodeOverride).Should().Be(expectedResponseCode);
        }

        [Fact]
        public async Task ExecuteRequestAsync_AuthorizationAndOtherErrors_SetsErrorStatusAndRecordsException()
        {
            using var activity = StartActivity();

            await CreateMiddleware([AuthorizationError.AnonymousAccessDenied(), new ExecutionError("Boom")]).ExecuteAsync();

            activity.Status.Should().Be(ActivityStatusCode.Error);
            activity.Events.Should().ContainSingle(x => x.Name == "exception");
            activity.GetTagItem(TelemetryTags.ResponseStatusCodeOverride).Should().Be(500);
        }

        [Fact]
        public async Task ExecuteRequestAsync_SingleOtherError_SetsErrorStatusWithItsMessage()
        {
            using var activity = StartActivity();

            await CreateMiddleware([new ExecutionError("Boom")]).ExecuteAsync();

            activity.Status.Should().Be(ActivityStatusCode.Error);
            activity.StatusDescription.Should().Be("Boom");
            activity.Events.Should().ContainSingle(x => x.Name == "exception");
            activity.GetTagItem(TelemetryTags.ResponseStatusCodeOverride).Should().Be(500);
        }

        private static Activity StartActivity()
        {
            // Activity.Current is set without any listener when started directly
            return new Activity("test").Start();
        }

        private static TestMiddleware CreateMiddleware(ExecutionError[] errors)
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

            return new TestMiddleware(documentExecuter.Object);
        }

        private sealed class TestMiddleware(IDocumentExecuter<ISchema> documentExecuter)
            : GraphQLHttpMiddlewareWithLogs<ISchema>(
                _ => Task.CompletedTask,
                new GraphQLSerializer(),
                documentExecuter,
                Mock.Of<IServiceScopeFactory>(),
                new GraphQLHttpMiddlewareOptions(),
                Mock.Of<IHostApplicationLifetime>(),
                NullLogger<GraphQLHttpMiddlewareWithLogs<ISchema>>.Instance)
        {
            public Task<ExecutionResult> ExecuteAsync()
            {
                var context = new DefaultHttpContext();
                context.Request.Method = HttpMethods.Post;
                context.Request.Path = "/graphql";

                return ExecuteRequestAsync(context, new GraphQLRequest { OperationName = "Test" }, Mock.Of<IServiceProvider>(), new Dictionary<string, object>());
            }
        }
    }
}
