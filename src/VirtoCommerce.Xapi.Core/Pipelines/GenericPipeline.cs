using Microsoft.Extensions.Options;
using PipelineNet.MiddlewareResolver;
using PipelineNet.Pipelines;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Xapi.Core.Pipelines
{
    public class GenericPipeline<TParameter> : AsyncPipeline<TParameter> where TParameter : class
    {
        public GenericPipeline(IOptions<GenericPipelineOptions<TParameter>> options, IMiddlewareResolver middlewareResolver) : base(middlewareResolver)
        {
            options.Value.Middlewares.Apply(middleware => Add(middleware));
        }

    }
}
