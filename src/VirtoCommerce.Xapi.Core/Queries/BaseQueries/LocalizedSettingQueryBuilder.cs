using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Queries.BaseQueries;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Xapi.Core.Queries;

public abstract class LocalizedSettingQueryBuilder<TQuery> : QueryBuilder<TQuery, LocalizedSettingResponse, LocalizedSettingResponseType>
    where TQuery : LocalizedSettingQuery
{
    protected LocalizedSettingQueryBuilder(IAuthorizationService authorizationService)
        : base(authorizationService)
    {
    }

    [Obsolete("Use the constructor without IMediator. The mediator is resolved from context.RequestServices per request.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    protected LocalizedSettingQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
        : this(authorizationService)
    {
    }
}
