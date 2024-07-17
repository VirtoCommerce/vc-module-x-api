using GraphQL.Server;
using Microsoft.Extensions.DependencyInjection;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    /// <summary>
    /// Custom implementation of GraphQLBuilder to call GraphQL.Server extensions methods
    /// </summary>
    public sealed class CustomGraphQLBuilder : IGraphQLBuilder
    {
        public IServiceCollection Services { get; }

        public CustomGraphQLBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
