using GraphQL.Server.Ui.Playground;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using VirtoCommerce.Xapi.Core.Models;
using static VirtoCommerce.Xapi.Core.ModuleConstants;

namespace VirtoCommerce.Xapi.Core.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSchemaGraphQL<TSchema>(this IApplicationBuilder builder, IConfiguration configuration = null, string schemaPath = null)
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

        var isSchemaIntrospectionEnabled = false;
        if (configuration != null)
        {
            isSchemaIntrospectionEnabled = configuration.GetValue<bool>($"{ConfigKeys.GraphQlPlayground}:{nameof(GraphQLPlaygroundOptions.Enable)}");
        }

        builder.UseGraphQL<TSchema>(path: graphQlPath);
        if (isSchemaIntrospectionEnabled)
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
