using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Xapi.Core.Extensions;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Extensions
{
    public class GraphQLBuilderExtensionsTests
    {
        [Fact]
        public async Task AddSchema_ConfigureMediatREnablesPreProcessors_PreProcessorRunsBeforeHandler()
        {
            var services = new ServiceCollection();
            services.AddGraphQL(builder => builder.AddSchema(services, typeof(GraphQLBuilderExtensionsTests), cfg =>
            {
                cfg.AutoRegisterRequestProcessors = true;
                cfg.AddOpenBehavior(typeof(RequestPreProcessorBehavior<,>));
            }));

            var result = await services.BuildServiceProvider().GetRequiredService<IMediator>().Send(new ProbeRequest());

            result.Should().Be(ProbeRequestPreProcessor.Marker);
        }

        [Fact]
        public async Task AddSchema_WithoutConfigureMediatR_HandlerRunsAndPreProcessorDoesNot()
        {
            var services = new ServiceCollection();
            services.AddGraphQL(builder => builder.AddSchema(services, typeof(GraphQLBuilderExtensionsTests)));

            var result = await services.BuildServiceProvider().GetRequiredService<IMediator>().Send(new ProbeRequest());

            result.Should().BeNull();
        }
    }

    public class ProbeRequest : IRequest<string>
    {
        public string Marker { get; set; }
    }

    public class ProbeRequestHandler : IRequestHandler<ProbeRequest, string>
    {
        public Task<string> Handle(ProbeRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.Marker);
        }
    }

    public class ProbeRequestPreProcessor : IRequestPreProcessor<ProbeRequest>
    {
        public const string Marker = "pre-processed";

        public Task Process(ProbeRequest request, CancellationToken cancellationToken)
        {
            request.Marker = Marker;

            return Task.CompletedTask;
        }
    }
}
