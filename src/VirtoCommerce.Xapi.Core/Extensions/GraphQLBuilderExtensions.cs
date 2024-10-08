using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Server;
using GraphQL.Server.Transports.Subscriptions.Abstractions;
using GraphQL.Server.Transports.WebSockets;
using GraphQL.Validation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Xapi.Core.Subscriptions.Infrastructure;

namespace VirtoCommerce.Xapi.Core.Extensions
{
    public static class GraphQLBuilderExtensions
    {
        public static IGraphQLBuilder ReplaceValidationRule<TOldRule, TNewRule>(this IGraphQLBuilder builder)
            where TOldRule : class, IValidationRule
            where TNewRule : class, IValidationRule
        {
            var coreRules = DocumentValidator.CoreRules as List<IValidationRule>;
            var oldRuleType = typeof(TOldRule);
            var oldRule = coreRules?.FirstOrDefault(x => x.GetType() == oldRuleType);

            if (oldRule != null)
            {
                coreRules.Remove(oldRule);
                builder.AddCustomValidationRule<TNewRule>();
            }

            return builder;
        }

        public static IGraphQLBuilder AddCustomValidationRule<TRule>(this IGraphQLBuilder builder)
            where TRule : class, IValidationRule
        {
            builder.Services.AddSingleton<IValidationRule, TRule>();

            return builder;
        }

        public static IGraphQLBuilder AddCustomValidationRule(this IGraphQLBuilder builder, Type ruleType)
        {
            builder.Services.AddSingleton(typeof(IValidationRule), ruleType);

            return builder;
        }

        public static IGraphQLBuilder AddSchema(this IGraphQLBuilder builder, Type coreAssemblyMarker, Type dataAssemblyMarker)
        {
            builder.AddGraphTypes(coreAssemblyMarker);
            builder.Services.AddMediatR(coreAssemblyMarker, dataAssemblyMarker);
            builder.Services.AddAutoMapper(coreAssemblyMarker, dataAssemblyMarker);
            builder.Services.AddSchemaBuilders(dataAssemblyMarker);

            return builder;
        }

        /// <summary>
        /// Add required services for GraphQL web sockets with custom IWebSocketConnectionFactory implementation
        /// </summary>
        public static IGraphQLBuilder AddCustomWebSockets(this IGraphQLBuilder builder)
        {
            builder.Services
                .AddTransient(typeof(IWebSocketConnectionFactory<>), typeof(CustomWebSocketConnectionFactory<>))
                .AddTransient<IOperationMessageListener, LogMessagesListener>()
                .AddTransient<IOperationMessageListener, ProtocolMessageListener>()
                .AddTransient<IOperationMessageListener, KeepAliveResolver>()
                .AddTransient<IOperationMessageListener, SubscriptionsUserContextResolver>();

            return builder;
        }
    }
}
