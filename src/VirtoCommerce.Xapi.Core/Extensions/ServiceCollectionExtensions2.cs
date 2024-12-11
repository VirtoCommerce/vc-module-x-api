//using System;
//using System.Collections.Generic;
//using System.Linq;
//using GraphQL.Server.Transports.Subscriptions.Abstractions;
//using GraphQL.Server.Transports.WebSockets;
//using GraphQL.Validation;
//using MediatR;
//using Microsoft.Extensions.DependencyInjection;
//using VirtoCommerce.Xapi.Core.Subscriptions.Infrastructure;

//namespace VirtoCommerce.Xapi.Core.Extensions
//{
//    public static class ServiceCollectionExtensions2
//    {
//        public static IServiceCollection ReplaceValidationRule<TOldRule, TNewRule>(this IServiceCollection services)
//            where TOldRule : class, IValidationRule
//            where TNewRule : class, IValidationRule
//        {
//            var coreRules = DocumentValidator.CoreRules as List<IValidationRule>;
//            var oldRuleType = typeof(TOldRule);
//            var oldRule = coreRules?.FirstOrDefault(x => x.GetType() == oldRuleType);

//            if (oldRule != null)
//            {
//                coreRules.Remove(oldRule);
//                services.AddCustomValidationRule<TNewRule>();
//            }

//            return services;
//        }

//        public static IServiceCollection AddCustomValidationRule<TRule>(this IServiceCollection services)
//            where TRule : class, IValidationRule
//        {
//            services.AddSingleton<IValidationRule, TRule>();

//            return services;
//        }

//        public static IServiceCollection AddCustomValidationRule(this IServiceCollection services, Type ruleType)
//        {
//            services.AddSingleton(typeof(IValidationRule), ruleType);

//            return services;
//        }

//        public static IServiceCollection AddSchema(this IServiceCollection services, Type coreAssemblyMarker, Type dataAssemblyMarker)
//        {
//            ////services.AddGraphTypes(coreAssemblyMarker);
//            services.AddMediatR(coreAssemblyMarker, dataAssemblyMarker);
//            services.AddAutoMapper(coreAssemblyMarker, dataAssemblyMarker);
//            services.AddSchemaBuilders(dataAssemblyMarker);

//            return services;
//        }

//        /// <summary>
//        /// Add required services for GraphQL web sockets with custom IWebSocketConnectionFactory implementation
//        /// </summary>
//        public static IServiceCollection AddCustomWebSockets(this IServiceCollection services)
//        {
//            services
//                .AddTransient(typeof(IWebSocketConnectionFactory<>), typeof(CustomWebSocketConnectionFactory<>))
//                .AddTransient<IOperationMessageListener, LogMessagesListener>()
//                .AddTransient<IOperationMessageListener, ProtocolMessageListener>()
//                .AddTransient<IOperationMessageListener, KeepAliveResolver>()
//                .AddTransient<IOperationMessageListener, SubscriptionsUserContextResolver>();

//            return services;
//        }
//    }
//}
