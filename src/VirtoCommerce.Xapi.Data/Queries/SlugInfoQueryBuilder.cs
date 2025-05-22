using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Queries;
using VirtoCommerce.Xapi.Data.Schemas;

namespace VirtoCommerce.Xapi.Data.Queries;

public class SlugInfoQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
    : QueryBuilder<SlugInfoQuery, SlugInfoResponse, SlugInfoResponseType>(mediator, authorizationService)
{
    protected override string Name => "slugInfo";
}
