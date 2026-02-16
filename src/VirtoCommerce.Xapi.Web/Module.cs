using System;
using System.IO;
using GraphQL;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Types;
using GraphQL.Validation.Rules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.DeveloperTools;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
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

        private bool IsComplexetyValidationEnabled
        {
            get
            {
                return Configuration.GetValue<bool>($"{ConfigKeys.GraphQlComplexetyValidation}:{nameof(GraphQLComplexetyValidationOptions.Enable)}");
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
            serviceCollection.AddGraphQL(builder =>
            {
                builder
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
                    builder.ReplaceValidationRule<KnownTypeNames, CustomKnownTypeNames>();
                    builder.ReplaceValidationRule<FieldsOnCorrectType, CustomFieldsOnCorrectType>();
                    builder.ReplaceValidationRule<KnownArgumentNames, CustomKnownArgumentNames>();
                }

                if (IsComplexetyValidationEnabled)
                {
                    builder.AddComplexityAnalyzer((options, serviceProvider) =>
                    {
                        var validationOptions = serviceProvider.GetRequiredService<IOptions<GraphQLComplexetyValidationOptions>>().Value;

                        options.MaxDepth = validationOptions.MaxDepth;
                        options.MaxComplexity = validationOptions.MaxComplexity;

                        options.DefaultScalarImpact = validationOptions.ScalarFieldImpact ?? options.DefaultScalarImpact;
                        options.DefaultObjectImpact = validationOptions.ObjectFieldImpact ?? options.DefaultObjectImpact;
                        options.DefaultListImpactMultiplier = validationOptions.ListImpactMultiplied ?? options.DefaultListImpactMultiplier;

                        if (!validationOptions.IgnoreIntrospection)
                        {
                            return;
                        }

                        options.ValidateComplexityDelegate = async (context) =>
                        {
                            if (context.ValidationContext.IsIntrospectionRequest())
                            {
                                // ignore complexity errors
                                context.Error = null;
                            }
                        };
                    });
                }
            });

            if (IsSchemaIntrospectionEnabled)
            {
                serviceCollection.AddTransient<Func<GraphiQLOptions>>(_ => () => new GraphiQLOptions
                {
                    IndexStream = _ =>
                        File.OpenRead(Path.Combine(ModuleInfo.FullPhysicalPath, "Content/graphiql/index.html")),
                });
            }

            //Register xApi boundaries
            serviceCollection.AddXCore(Configuration);

            serviceCollection.AddAutoMapper(ModuleInfo.Assembly);

            serviceCollection.Configure<GraphQLPlaygroundOptions>(Configuration.GetSection(ConfigKeys.GraphQlPlayground));
            serviceCollection.Configure<GraphQLWebSocketOptions>(Configuration.GetSection(ConfigKeys.GraphQlWebSocket));
            serviceCollection.Configure<GraphQLComplexetyValidationOptions>(Configuration.GetSection(ConfigKeys.GraphQlComplexetyValidation));
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
            appBuilder.UseSchemaGraphQL<ISchema>(IsSchemaIntrospectionEnabled);

            // settings
            var settingsRegistrar = serviceProvider.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(Settings.General.AllSettings, ModuleInfo.Id);
            settingsRegistrar.RegisterSettingsForType(Settings.StoreLevelSettings, nameof(Store));

            if (IsSchemaIntrospectionEnabled)
            {
                // developer tool
                var developerToolRegistrar = serviceProvider.GetRequiredService<IDeveloperToolRegistrar>();
                developerToolRegistrar.RegisterDeveloperTool(new DeveloperToolDescriptor { Name = "GraphQL", Url = "/ui/graphiql", SortOrder = 40 });
            }
        }

        public void Uninstall()
        {
            // Method intentionally left empty.
        }
    }
}
