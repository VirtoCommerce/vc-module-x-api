using GraphQL;
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
using VirtoCommerce.Xapi.Core.Subscriptions.Infrastructure;
using VirtoCommerce.Xapi.Data;
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

            //serviceCollection.AddTransient(typeof(IGraphQLExecuter<>), typeof(CustomGraphQLExecuter<>)); // need to check how to add AppInsight telemetry later
            //serviceCollection.AddSingleton<IDocumentExecuter, SubscriptionDocumentExecuter>(); // removed, need to check what to do with subscriptions later

#pragma warning disable CS0618 // Type or member is obsolete
            GlobalSwitches.UseLegacyTypeNaming = true;
#pragma warning restore CS0618 // Type or member is obsolete

            //Register .NET GraphQL server
            serviceCollection.AddGraphQL(bulder =>
            {
                bulder
                    //.AddHttpMiddleware<ISchema, GraphQLHttpMiddleware<ISchema>>() // how to do AI telemetry here?
                    .AddNewtonsoftJson()
                    .AddSchema(serviceCollection, typeof(CoreAssemblyMarker), typeof(DataAssemblyMarker))
                    .AddPermissionAuthorization()
                    .ConfigureExecutionOptions(options =>
                    {
                        options.EnableMetrics = false;
                    })
                    .AddErrorInfoProvider(options =>
                    {
                        options.ExposeExtensions = true;
                        options.ExposeExceptionDetails = true;
                    })
                    .AddUserContextBuilder(async context => await context.BuildGraphQLUserContextAsync())
                    .AddDataLoader()
                    .AddValidationRule<ContentTypeValidationRule>()
                    .AddWebSocketAuthentication<SubscriptionsUserContextResolver>();
                //.AddCustomWebSockets() // disabled for now, rewrite later

                if (!IsSchemaIntrospectionEnabled)
                {
                    bulder.ReplaceValidationRule<KnownTypeNames, CustomKnownTypeNames>();
                    bulder.ReplaceValidationRule<FieldsOnCorrectType, CustomFieldsOnCorrectType>();
                    bulder.ReplaceValidationRule<KnownArgumentNames, CustomKnownArgumentNames>();
                }
            });

            //var graphQlBuilder = serviceCollection.AddGraphQL(options =>
            //{
            //    options.EnableMetrics = false;
            //})
            //.AddNewtonsoftJson(deserializerSettings => { }, serializerSettings => { })
            //.AddErrorInfoProvider(options =>
            //{
            //    options.ExposeExtensions = true;
            //    options.ExposeExceptionStackTrace = true;
            //})
            //.AddUserContextBuilder(async context => await context.BuildGraphQLUserContextAsync())
            //.AddRelayGraphTypes() // removed
            //.AddCustomWebSockets()
            //.AddDataLoader()
            //.AddCustomValidationRule<ContentTypeValidationRule>();

            //if (!IsSchemaIntrospectionEnabled)
            //{
            //    graphQlBuilder.ReplaceValidationRule<KnownTypeNames, CustomKnownTypeNames>();
            //    graphQlBuilder.ReplaceValidationRule<FieldsOnCorrectType, CustomFieldsOnCorrectType>();
            //    graphQlBuilder.ReplaceValidationRule<KnownArgumentNames, CustomKnownArgumentNames>();
            //}

            //Register xApi boundaries
            serviceCollection.AddXCore(Configuration);

            serviceCollection.AddAutoMapper(ModuleInfo.Assembly);

            serviceCollection.Configure<GraphQLPlaygroundOptions>(Configuration.GetSection(ConfigKeys.GraphQlPlayground));
            serviceCollection.Configure<GraphQLWebSocketOptions>(Configuration.GetSection(ConfigKeys.GraphQlWebSocket));
            serviceCollection.Configure<StoresOptions>(Configuration.GetSection(ConfigKeys.Stores));
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            var serviceProvider = appBuilder.ApplicationServices;

            // disable web sockets/subscription for now
            // this is required for websockets support
            appBuilder.UseWebSockets();

            // use websocket middleware for ISchema at default path /graphql
            //appBuilder.UseGraphQLWebSockets<ISchema>();

            // add http for Schema at default url /graphql
            // use GraphQL Playground at default URL /ui/playground
            appBuilder.UseSchemaGraphQL<ISchema>(IsSchemaIntrospectionEnabled);

            // settings
            var settingsRegistrar = serviceProvider.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(Settings.General.AllSettings, ModuleInfo.Id);
            settingsRegistrar.RegisterSettingsForType(Settings.StoreLevelSettings, nameof(Store));
        }

        public void Uninstall()
        {
            // Method intentionally left empty.
        }
    }
}
