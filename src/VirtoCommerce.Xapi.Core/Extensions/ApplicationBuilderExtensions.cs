using GraphQL.Server.Ui.Playground;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;

namespace VirtoCommerce.Xapi.Core.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSchemaGraphQL<TSchema>(this IApplicationBuilder builder, bool schemaIntrospectionEnabled = true, string schemaPath = null)
        where TSchema : ISchema
    {
        var graphQlPath = "/graphql";
        if (!string.IsNullOrEmpty(schemaPath))
        {
            graphQlPath = $"{graphQlPath}/{schemaPath}";
        }

        var playgroundPath = "/ui/playground";
        if (!string.IsNullOrEmpty(schemaPath))
        {
            playgroundPath = $"{playgroundPath}/{schemaPath}";
        }

        builder.UseGraphQL<TSchema>(path: graphQlPath);
        if (schemaIntrospectionEnabled)
        {
            builder.UseGraphQLPlayground(new PlaygroundOptions
            {
                GraphQLEndPoint = graphQlPath,
            },
            path: playgroundPath);
        }

        return builder;
    }
}
