using GraphQL.Server.Ui.Playground;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseScopedSchema<TMarker>(this IApplicationBuilder builder, string schemaPath)
    {
        var playgroundOptions = builder.ApplicationServices.GetService<IOptions<GraphQLPlaygroundOptions>>();

        return builder.UseSchemaGraphQL<ScopedSchemaFactory<TMarker>>(playgroundOptions?.Value?.Enable ?? true, schemaPath);
    }

    public static IApplicationBuilder UseSchemaGraphQL<TSchema>(this IApplicationBuilder builder, bool schemaIntrospectionEnabled = true, string schemaPath = null)
        where TSchema : ISchema
    {
        var graphQlPath = "/graphql";
        if (!string.IsNullOrEmpty(schemaPath))
        {
            graphQlPath = $"{graphQlPath}/{schemaPath}";
        }

        builder.UseGraphQL<TSchema>(path: graphQlPath);
        if (schemaIntrospectionEnabled)
        {
            var playgroundPath = "/ui/playground";
            if (!string.IsNullOrEmpty(schemaPath))
            {
                playgroundPath = $"{playgroundPath}/{schemaPath}";
            }


#pragma warning disable CS0618 // Type or member is obsolete
            builder.UseGraphQLPlayground(playgroundPath,
                new PlaygroundOptions
                {
                    GraphQLEndPoint = graphQlPath,
                });
#pragma warning restore CS0618 // Type or member is obsolete
        }

        return builder;
    }
}
