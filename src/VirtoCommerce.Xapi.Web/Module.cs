using System.IO;
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

#pragma warning disable CS0618 // Type or member is obsolete
            // Use legacy type naming for backward compatibility
            GlobalSwitches.UseLegacyTypeNaming = true;
            GlobalSwitches.InferFieldNullabilityFromNRTAnnotations = false;
#pragma warning restore CS0618 // Type or member is obsolete

            // Register .NET GraphQL server
            serviceCollection.AddGraphQL(bulder =>
            {
                bulder
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

                if (!IsSchemaIntrospectionEnabled)
                {
                    bulder.ReplaceValidationRule<KnownTypeNames, CustomKnownTypeNames>();
                    bulder.ReplaceValidationRule<FieldsOnCorrectType, CustomFieldsOnCorrectType>();
                    bulder.ReplaceValidationRule<KnownArgumentNames, CustomKnownArgumentNames>();
                }
            });


            //Register xApi boundaries
            serviceCollection.AddXCore(Configuration);

            serviceCollection.AddAutoMapper(ModuleInfo.Assembly);

            serviceCollection.Configure<GraphQLPlaygroundOptions>(Configuration.GetSection(ConfigKeys.GraphQlPlayground));
            serviceCollection.Configure<GraphQLWebSocketOptions>(Configuration.GetSection(ConfigKeys.GraphQlWebSocket));
            serviceCollection.Configure<StoresOptions>(Configuration.GetSection(ConfigKeys.Stores));

            serviceCollection.AddAuthenticationFilter(Configuration);
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            var serviceProvider = appBuilder.ApplicationServices;

            appBuilder.UseAuthenticationFilter();

            // this is required for websockets support
            appBuilder.UseWebSockets();

            // add http for Schema at default url /graphql
            // use GraphQL Playground at default URL /ui/playground

            var graphiqlFilePath = Path.Combine(ModuleInfo.FullPhysicalPath, "Content/graphiql/index.html");
            appBuilder.UseSchemaGraphQL<ISchema>(graphiqlFilePath, IsSchemaIntrospectionEnabled);

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
