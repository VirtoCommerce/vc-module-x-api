using GraphQL;
using GraphQL.Server;
using GraphQL.Types;
using GraphQL.Validation.Rules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.TaxModule.Core.Model;
using VirtoCommerce.Xapi.Core;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Infrastructure.Validation;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Subscriptions;
using VirtoCommerce.Xapi.Data.Extensions;
using VirtoCommerce.Xapi.Web.Extensions;
using static VirtoCommerce.Xapi.Core.ModuleConstants;

namespace VirtoCommerce.Xapi.Web
{
    public class Module : IModule, IHasConfiguration
    {
        public ManifestModuleInfo ModuleInfo { get; set; }
        public IConfiguration Configuration { get; set; }

        private bool IsSchemaIntrospectionEnabled
        {
            get
            {
                return Configuration.GetValue<bool>($"{ConfigKeys.GraphQlPlayground}:{nameof(GraphQLPlaygroundOptions.Enable)}");
            }
        }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddApplicationInsightsTelemetryProcessor<IgnorePlainCoreXapiGraphQLTelemetryProcessor>();
            // register custom executor with app insight wrapper
            serviceCollection.AddTransient(typeof(IGraphQLExecuter<>), typeof(CustomGraphQLExecuter<>));
            serviceCollection.AddSingleton<IDocumentExecuter, SubscriptionDocumentExecuter>();

            //Register .NET GraphQL server
            var graphQlBuilder = serviceCollection.AddGraphQL(options =>
            {
                options.EnableMetrics = false;
            })
            .AddNewtonsoftJson(deserializerSettings => { }, serializerSettings => { })
            .AddErrorInfoProvider(options =>
            {
                options.ExposeExtensions = true;
                options.ExposeExceptionStackTrace = true;
            })
            .AddUserContextBuilder(async context => await context.BuildGraphQLUserContextAsync())
            .AddRelayGraphTypes()
            .AddCustomWebSockets()
            .AddDataLoader()
            .AddCustomValidationRule<ContentTypeValidationRule>();

            if (!IsSchemaIntrospectionEnabled)
            {
                graphQlBuilder.ReplaceValidationRule<KnownTypeNames, CustomKnownTypeNames>();
                graphQlBuilder.ReplaceValidationRule<FieldsOnCorrectType, CustomFieldsOnCorrectType>();
                graphQlBuilder.ReplaceValidationRule<KnownArgumentNames, CustomKnownArgumentNames>();
            }

            //Register xApi boundaries
            serviceCollection.AddXCore(graphQlBuilder, Configuration);

            serviceCollection.AddAutoMapper(ModuleInfo.Assembly);

            serviceCollection.Configure<GraphQLPlaygroundOptions>(Configuration.GetSection(ConfigKeys.GraphQlPlayground));
            serviceCollection.Configure<GraphQLWebSocketOptions>(Configuration.GetSection(ConfigKeys.GraphQlWebSocket));
            serviceCollection.Configure<StoresOptions>(Configuration.GetSection(ConfigKeys.Stores));
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            var serviceProvider = appBuilder.ApplicationServices;

            // this is required for websockets support
            appBuilder.UseWebSockets();

            // use websocket middleware for ISchema at default path /graphql
            appBuilder.UseGraphQLWebSockets<ISchema>();

            // add http for Schema at default url /graphql
            // use GraphQL Playground at default URL /ui/playground
            appBuilder.UseSchemaGraphQL<ISchema>(Configuration);

            // settings
            var settingsRegistrar = serviceProvider.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);
            settingsRegistrar.RegisterSettingsForType(ModuleConstants.Settings.StoreLevelSettings, nameof(Store));
        }

        public void Uninstall()
        {
            // Method intentionally left empty.
        }
    }
}
