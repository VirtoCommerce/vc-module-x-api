using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Server.Transports.AspNetCore.WebSockets;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            WebSockets = new GraphQLWebSocketOptions()
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

            builder.UseGraphQLGraphiQL(path: graphiqlPath,
                new GraphiQLOptions
                {
                    GraphQLEndPoint = graphQlPath,
                    SubscriptionsEndPoint = graphQlPath,
                    IndexStream = _ =>
                    {
                        var originalStream = new GraphiQLOptions().IndexStream(null);
                        using (var reader = new StreamReader(originalStream, leaveOpen: true))
                        {
                            var content = reader.ReadToEnd();

                            var pathsDictionary = new Dictionary<string, string>
                            {
                                { "https://unpkg.com/graphiql@3.2.0/graphiql.min.css", null },
                                { "https://unpkg.com/graphiql@3.2.0/graphiql.min.js", null }
                            };

                            var modifiedContent = "<!-- Using local paths --!>\n" + content;

                            // Replace CDN paths with local paths
                            foreach (var item in pathsDictionary.Where(x => x.Value != null))
                            {
                                var cdnPackagePath = item.Key;
                                var newPackagePath = item.Value;

                                modifiedContent = modifiedContent.Replace(cdnPackagePath, newPackagePath);
                            }

                            var modifiedStream = new MemoryStream();
                            using (var writer = new StreamWriter(modifiedStream, leaveOpen: true))
                            {
                                writer.Write(modifiedContent);
                                writer.Flush();

                                modifiedStream.Position = 0;
                            }

                            return modifiedStream;
                        }
                    }
                });
        }

        return builder;
    }
}
