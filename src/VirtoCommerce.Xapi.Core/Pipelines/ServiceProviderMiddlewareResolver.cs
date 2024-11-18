using System;
using PipelineNet.MiddlewareResolver;

namespace VirtoCommerce.Xapi.Core.Pipelines
{
    public class ServiceProviderMiddlewareResolver : IMiddlewareResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderMiddlewareResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public MiddlewareResolverResult Resolve(Type type)
        {
            var result = _serviceProvider.GetService(type);
            return new MiddlewareResolverResult { IsDisposable = true, Middleware = result };
        }
    }
}
