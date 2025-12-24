using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Xapi.Core.Infrastructure;

public class CustomSchema : Schema
{
    private readonly List<IGraphTypeHook> _graphTypeHooks;

    public CustomSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        var graphTypeHooks = serviceProvider.GetServices<IGraphTypeHook>();
        _graphTypeHooks = graphTypeHooks?.ToList() ?? [];
    }

    protected override void OnBeforeInitializeType(IGraphType graphType)
    {
        var typeHooks = _graphTypeHooks
            .Where(x => x.TypeName.EqualsIgnoreCase(graphType.Name));

        foreach (var typeHook in typeHooks)
        {
            typeHook.BeforeTypeInitialized(graphType);
        }
    }
}
