using System;
using System.Linq;
using System.Reflection;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Infrastructure.Internal;

namespace VirtoCommerce.Xapi.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static ISchemaTypeBuilder<TSchemaType> AddSchemaType<TSchemaType>(this IServiceCollection services) where TSchemaType : class, IGraphType
        {
            //Register GraphQL Schema type in the ServicesCollection
            services.TryAddTransient<TSchemaType>();

            return new SchemaTypeBuilder<TSchemaType>(services);
        }

        public static void AddSchemaBuilder<TSchemaBuilder>(this IServiceCollection services) where TSchemaBuilder : class, ISchemaBuilder
        {
            services.AddSingleton<ISchemaBuilder, TSchemaBuilder>();
        }

        public static void AddSchemaBuilders(this IServiceCollection services)
        {
            services.AddSchemaBuilders(Assembly.GetCallingAssembly());
        }

        public static void AddSchemaBuilders(this IServiceCollection services, Type assemblyMarkerType)
        {
            services.AddSchemaBuilders(assemblyMarkerType.Assembly);
        }

        public static void AddSchemaBuilders(this IServiceCollection services, Assembly assembly)
        {
            var schemaBuilder = typeof(ISchemaBuilder);

            foreach (var type in assembly.GetTypes().Where(x => !x.IsAbstract && x.IsAssignableTo(schemaBuilder)))
            {
                services.TryAddEnumerable(new ServiceDescriptor(schemaBuilder, type, ServiceLifetime.Singleton));
            }
        }

        public static void OverrideSchemaBuilder<TOld, TNew>(this IServiceCollection serviceCollection)
            where TOld : class, ISchemaBuilder
            where TNew : class, ISchemaBuilder, TOld
        {
            var descriptor = serviceCollection.FirstOrDefault(x =>
                x.ServiceType == typeof(ISchemaBuilder) &&
                x.ImplementationType == typeof(TOld));

            if (descriptor != null)
            {
                serviceCollection.Remove(descriptor);
                serviceCollection.AddSchemaBuilder<TNew>();
            }
        }

        public static void OverrideGraphType<TOld, TNew>(this IServiceCollection services)
            where TOld : class, IGraphType
            where TNew : class, IGraphType, TOld
        {
            services.TryAddTransient<TNew>();
            AbstractTypeFactory<IGraphType>.OverrideType<TOld, TNew>();
        }
    }
}
