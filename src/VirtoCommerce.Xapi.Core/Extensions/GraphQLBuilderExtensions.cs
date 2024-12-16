using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.DI;
using GraphQL.Validation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ServiceLifetime = GraphQL.DI.ServiceLifetime;

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
                builder.AddValidationRule<TNewRule>();
            }

            return builder;
        }

        public static IGraphQLBuilder AddCustomValidationRule<TRule>(this IGraphQLBuilder builder)
            where TRule : class, IValidationRule
        {
            builder.Services.Register<IValidationRule, TRule>(ServiceLifetime.Singleton);

            return builder;
        }

        public static IGraphQLBuilder AddCustomValidationRule(this IGraphQLBuilder builder, Type ruleType)
        {
            builder.Services.Register(typeof(IValidationRule), ruleType);

            return builder;
        }

        public static IGraphQLBuilder AddSchema(this IGraphQLBuilder builder, IServiceCollection services, Type coreAssemblyMarker, Type dataAssemblyMarker)
        {
            builder.AddGraphTypes(coreAssemblyMarker.Assembly);
            services.AddMediatR(coreAssemblyMarker, dataAssemblyMarker);
            services.AddAutoMapper(coreAssemblyMarker, dataAssemblyMarker);
            services.AddSchemaBuilders(dataAssemblyMarker);

            return builder;
        }

        ///// <summary>
        ///// Add required services for GraphQL web sockets with custom IWebSocketConnectionFactory implementation
        ///// </summary>
        //public static IGraphQLBuilder AddCustomWebSockets(this IGraphQLBuilder builder)
        //{
        //    builder.Services
        //        .Register(typeof(IWebSocketConnectionFactory<>), typeof(CustomWebSocketConnectionFactory<>), ServiceLifetime.Transient)
        //        .Register<IOperationMessageListener, LogMessagesListener>(ServiceLifetime.Transient)
        //        .Register<IOperationMessageListener, ProtocolMessageListener>(ServiceLifetime.Transient)
        //        .Register<IOperationMessageListener, KeepAliveResolver>(ServiceLifetime.Transient)
        //        .Register<IOperationMessageListener, SubscriptionsUserContextResolver>(ServiceLifetime.Transient);

        //    return builder;
        //}
    }
}
