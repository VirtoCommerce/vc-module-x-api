using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using GraphQL.Types;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VirtoCommerce.Xapi.Core.BaseQueries;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.BaseQueries
{
    public class RequestBuilderTests
    {
        [Fact]
        public async Task GetResponseAsync_ResolvesMediatorFromRequestScope_NotFromConstructor()
        {
            var expectedResponse = new TestResponse { Value = "expected" };
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(x => x.Send(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var services = new ServiceCollection();
            services.AddSingleton(mediatorMock.Object);
            var provider = services.BuildServiceProvider();

            var sut = new TestQueryBuilder(Mock.Of<IAuthorizationService>());
            var context = new ResolveFieldContext<object>
            {
                RequestServices = provider,
            };

            var (request, response) = await sut.InvokeResolve(context);

            request.Should().NotBeNull();
            response.Should().BeSameAs(expectedResponse);
            mediatorMock.Verify(x => x.Send(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        private sealed class TestResponse
        {
            public string Value { get; set; }
        }

        private sealed class TestQuery : Query<TestResponse>
        {
            public override IEnumerable<QueryArgument> GetArguments()
            {
                yield break;
            }

            public override void Map(IResolveFieldContext context)
            {
            }
        }

        private sealed class TestQueryBuilder : QueryBuilder<TestQuery, TestResponse, StringGraphType>
        {
            protected override string Name => "test";

            public TestQueryBuilder(IAuthorizationService authorizationService)
                : base(authorizationService)
            {
            }

            public Task<(TestQuery Request, TestResponse Response)> InvokeResolve(IResolveFieldContext<object> context)
            {
                return Resolve(context);
            }
        }
    }
}
