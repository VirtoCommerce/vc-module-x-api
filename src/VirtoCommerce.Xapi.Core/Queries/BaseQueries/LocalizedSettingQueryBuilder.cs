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
    protected LocalizedSettingQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
        : base(mediator, authorizationService)
    {
    }
}
