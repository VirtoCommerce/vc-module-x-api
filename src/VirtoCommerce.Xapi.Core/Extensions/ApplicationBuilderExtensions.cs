using System;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Server.Transports.AspNetCore.WebSockets;
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

        builder.UseGraphQL<GraphQLHttpMiddlewareWithLogs<TSchema>>(path: graphQlPath, new GraphQLHttpMiddlewareOptions()
        {
            // configure keep-alive packets
            WebSockets = new GraphQLWebSocketOptions()
            {
                KeepAliveTimeout = TimeSpan.FromSeconds(10),  // pass the desired keep-alive timeout
                KeepAliveMode = KeepAliveMode.Interval,
            }
            // can set supported sub-protocols to only include desired protocols with options.WebSockets.SupportedWebSocketSubProtocols = ["graphql-ws"];
        });

        if (schemaIntrospectionEnabled)
        {
            var playgroundPath = "/ui/playground";
            if (!string.IsNullOrEmpty(schemaPath))
            {
                playgroundPath = $"{playgroundPath}/{schemaPath}";
            }

#pragma warning disable CS0618 // Type or member is obsolete
            // UI Playground
            builder.UseGraphQLPlayground(playgroundPath,
                new PlaygroundOptions
                {
                    GraphQLEndPoint = graphQlPath,
                    SubscriptionsEndPoint = graphQlPath,
                });
#pragma warning restore CS0618 // Type or member is obsolete

            // GraphiQL
            var graphiqlPath = "/ui/graphiql";
            builder.UseGraphQLGraphiQL(path: graphiqlPath,
                new GraphQL.Server.Ui.GraphiQL.GraphiQLOptions
                {
                    GraphQLEndPoint = graphQlPath,         // url of GraphQL endpoint
                    SubscriptionsEndPoint = graphQlPath,   // url of GraphQL endpoint
                });

            // Altair
            var altairPath = "/ui/altair";
            builder.UseGraphQLAltair(path: altairPath,
                new GraphQL.Server.Ui.Altair.AltairOptions
                {
                    GraphQLEndPoint = graphQlPath,         // url of GraphQL endpoint
                    SubscriptionsEndPoint = graphQlPath,   // url of GraphQL endpoint
                });
        }

        return builder;
    }
}
