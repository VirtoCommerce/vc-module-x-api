using System;
using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Xapi.Core.BaseQueries;

public abstract class QueryBuilder<TQuery, TResult, TResultGraphType>
    : RequestBuilder<TQuery, TResult, TResultGraphType>
    where TQuery : IQuery<TResult>, IExtendableQuery, IHasArguments
    where TResultGraphType : IGraphType
{
    protected QueryBuilder(IAuthorizationService authorizationService)
        : base(authorizationService)
    {
    }

    [Obsolete("Use the constructor without IMediator. The mediator is resolved from context.RequestServices per request.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    protected QueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
        : this(authorizationService)
    {
    }

    public override void Build(ISchema schema)
    {
        schema.Query.AddField(GetFieldType());
    }

    protected override IEnumerable<QueryArgument> GetArguments()
    {
        return AbstractTypeFactory<TQuery>.TryCreateInstance().GetArguments();
    }

    protected override TQuery GetRequest(IResolveFieldContext<object> context)
    {
        var request = AbstractTypeFactory<TQuery>.TryCreateInstance();
        request.Map(context);

        return request;
    }
}
