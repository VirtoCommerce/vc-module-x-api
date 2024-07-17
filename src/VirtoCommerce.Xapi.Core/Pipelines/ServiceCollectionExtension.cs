using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PipelineNet.MiddlewareResolver;

namespace VirtoCommerce.Xapi.Core.Pipelines
{
    public static class ServiceCollectionExtensions
    {
        public static GenericPipelineBuilder<TParameter> AddPipeline<TParameter>(this IServiceCollection services) where TParameter : class
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<IMiddlewareResolver, ServiceProviderMiddlewareResolver>();
            services.TryAddSingleton<IGenericPipelineLauncher, GenericPipelineLauncher>();

            services.TryAddTransient<GenericPipeline<TParameter>>();

            return new GenericPipelineBuilder<TParameter>(services);
        }


        public static IServiceCollection AddPipeline<TParameter>(this IServiceCollection services, Action<GenericPipelineBuilder<TParameter>> configuration) where TParameter : class
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            configuration(services.AddPipeline<TParameter>());

            return services;
        }
    }
}
