using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL;
using GraphQL.DI;
using GraphQL.Validation;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Xapi.Core.Models;
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

        public static IGraphQLBuilder AddSchema(this IGraphQLBuilder builder, IServiceCollection services, Type assemblyMarker)
        {
            builder.AddGraphTypes(assemblyMarker.Assembly);
            builder.AddOptionalGraphTypes(assemblyMarker);
            services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assemblyMarker.Assembly));
            services.AddAutoMapper(assemblyMarker);
            services.AddSchemaBuilders(assemblyMarker);

            return builder;
        }

        public static IGraphQLBuilder AddSchema(this IGraphQLBuilder builder, IServiceCollection services, Type coreAssemblyMarker, Type dataAssemblyMarker)
        {
            builder.AddGraphTypes(coreAssemblyMarker.Assembly);
            builder.AddOptionalGraphTypes(coreAssemblyMarker);
            services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblies(coreAssemblyMarker.Assembly, dataAssemblyMarker.Assembly));
            services.AddAutoMapper(coreAssemblyMarker, dataAssemblyMarker);
            services.AddSchemaBuilders(dataAssemblyMarker);

            return builder;
        }

        public static void AddOptionalGraphTypes(this IGraphQLBuilder builder, Type assemblyType)
        {
            var dependencyNames = assemblyType.GetCustomAttributes<OptionalGraphQlTypesContainerAttribute>()
                .Select(x => x.DependencyName)
                .Where(x => x != null)
                .ToArray();

            if (dependencyNames.Length > 0)
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var dependencyName in dependencyNames)
                {
                    var targetAssembly = loadedAssemblies.FirstOrDefault(x => x.GetName().Name.Equals(dependencyName, StringComparison.OrdinalIgnoreCase));

                    if (targetAssembly != null)
                    {
                        builder.AddGraphTypes(targetAssembly);
                    }
                }
            }
        }
    }
}
