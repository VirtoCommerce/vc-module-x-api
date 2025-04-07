using System;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Server.Transports.AspNetCore.WebSockets;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.DeveloperTools;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;
using static VirtoCommerce.Xapi.Core.ModuleConstants;

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
        var graphQlPath = string.IsNullOrEmpty(schemaPath)
            ? GraphQlPath
            : $"{GraphQlPath}/{schemaPath}";

        var webSocketOptions = builder.ApplicationServices.GetService<IOptions<Subscriptions.GraphQLWebSocketOptions>>();

        builder.UseGraphQL<GraphQLHttpMiddlewareWithLogs<TSchema>>(path: graphQlPath, new GraphQLHttpMiddlewareOptions()
        {
            // configure keep-alive packets
            WebSockets = new GraphQLWebSocketOptions
            {
                KeepAliveTimeout = webSocketOptions.Value.KeepAliveInterval,
            }
        });

        if (schemaIntrospectionEnabled)
        {
            // GraphiQL
            var graphiqlPath = "/ui/graphiql";
            if (!string.IsNullOrEmpty(schemaPath))
            {
                graphiqlPath = $"{graphiqlPath}/{schemaPath}";
            }

            var options = builder.ApplicationServices.GetRequiredService<Func<GraphiQLOptions>>()();

            options.GraphQLEndPoint = graphQlPath;
            options.SubscriptionsEndPoint = graphQlPath;

            builder.UseGraphQLGraphiQL(path: graphiqlPath, options);

            // developer tool
            var developerToolRegistrar = builder.ApplicationServices.GetRequiredService<IDeveloperToolRegistrar>();
            developerToolRegistrar.RegisterDeveloperTool(new DeveloperToolDescriptor { Name = "GraphQL", Url = "/ui/graphiql", SortOrder = 40 });
        }

        return builder;
    }
}
