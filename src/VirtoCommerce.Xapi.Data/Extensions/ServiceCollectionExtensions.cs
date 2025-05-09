using GraphQL.Introspection;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Core.Security.ExternalSignIn;
using VirtoCommerce.Platform.Security.OpenIddict;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Pipelines;
using VirtoCommerce.Xapi.Core.Services;
using VirtoCommerce.Xapi.Data.Security;
using VirtoCommerce.Xapi.Data.Services;
using static VirtoCommerce.Xapi.Core.ModuleConstants;
using ContactSignInValidator = VirtoCommerce.Xapi.Data.Security.OpenIddict.ContactSignInValidator;
using DynamicPropertyResolverService = VirtoCommerce.Xapi.Data.Services.DynamicPropertyResolverService;
using DynamicPropertyUpdaterService = VirtoCommerce.Xapi.Data.Services.DynamicPropertyUpdaterService;
using UserManagerCore = VirtoCommerce.Xapi.Data.Services.UserManagerCore;

namespace VirtoCommerce.Xapi.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthenticationFilter(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOptions<GraphQLOptions>().Bind(configuration.GetSection(ConfigKeys.GraphQl)).ValidateDataAnnotations();
            serviceCollection.AddScoped<AuthenticationFilterMiddleware>();

            return serviceCollection;
        }

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

        public static IServiceCollection AddXCore(this IServiceCollection services, IConfiguration configuration)
        {
            //Register custom GraphQL dependencies
            services.AddSingleton<ISchemaFilter, CustomSchemaFilter>();
            services.AddSingleton<ISchema, SchemaFactory>();

            services.AddTransient<IDynamicPropertyResolverService, DynamicPropertyResolverService>();
            services.AddTransient<IDynamicPropertyUpdaterService, DynamicPropertyUpdaterService>();
            services.AddTransient<IUserManagerCore, UserManagerCore>();
            services.AddTransient<ITokenRequestValidator, ContactSignInValidator>();
            services.AddTransient<IExternalSignInValidator, ExternalSignInValidator>();
            services.AddTransient<IExternalSignInUserBuilder, ExternalSignInUserBuilder>();

            services.AddTransient<ILoadUserToEvalContextService, LoadUserToEvalContextService>();
            services.AddDistributedLockService(configuration);

            services.AddPipeline<PipelineSeoInfoRequest>();

            return services;
        }
    }
}
