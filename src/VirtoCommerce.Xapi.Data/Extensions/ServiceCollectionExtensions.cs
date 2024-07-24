using GraphQL.Authorization;
using GraphQL.Introspection;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VirtoCommerce.Platform.Core.Security.ExternalSignIn;
using VirtoCommerce.Platform.Security.OpenIddict;
using VirtoCommerce.Xapi.Core;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Services;
using VirtoCommerce.Xapi.Data.Security.Authorization;
using VirtoCommerce.Xapi.Data.Services;
using ContactSignInValidator = VirtoCommerce.Xapi.Data.Security.OpenIddict.ContactSignInValidator;
using DynamicPropertyResolverService = VirtoCommerce.Xapi.Data.Services.DynamicPropertyResolverService;
using DynamicPropertyUpdaterService = VirtoCommerce.Xapi.Data.Services.DynamicPropertyUpdaterService;
using IGraphQLBuilder = GraphQL.Server.IGraphQLBuilder;
using UserManagerCore = VirtoCommerce.Xapi.Data.Services.UserManagerCore;

namespace VirtoCommerce.Xapi.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedLockService(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetConnectionString("RedisConnectionString");
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                services.AddSingleton<IDistributedLockService, DistributedLockService>();
            }
            else
            {
                services.AddSingleton<IDistributedLockService, NoLockService>();
            }

            return services;
        }

        public static IServiceCollection AddXCore(this IServiceCollection services, IGraphQLBuilder graphQlBuilder, IConfiguration configuration)
        {
            graphQlBuilder.AddSchema(typeof(CoreAssemblyMarker), typeof(DataAssemblyMarker));

            //Register custom GraphQL dependencies
            services.AddTransient<IValidationRule, AuthorizationValidationRule>();
            services.TryAddSingleton<IAuthorizationEvaluator, PermissionAuthorizationEvaluator>();

            services.AddSingleton<ISchemaFilter, CustomSchemaFilter>();
            services.AddSingleton<ISchema, SchemaFactory>();

            services.AddTransient<IDynamicPropertyResolverService, DynamicPropertyResolverService>();
            services.AddTransient<IDynamicPropertyUpdaterService, DynamicPropertyUpdaterService>();
            services.AddTransient<IUserManagerCore, UserManagerCore>();
            services.AddTransient<ITokenRequestValidator, ContactSignInValidator>();
            services.AddTransient<IExternalSignInValidator, ExternalSignInValidator>();
            services.AddTransient<IExternalSignInUserBuilder, ExternalSignInUserBuilder>();

            // provider for external fields
            services.AddSingleton<IExternalFieldProvider, ExternalFieldProvider>();

            services.AddTransient<ILoadUserToEvalContextService, LoadUserToEvalContextService>();
            services.AddDistributedLockService(configuration);

            return services;
        }
    }
}
