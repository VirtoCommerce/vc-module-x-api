using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Queries;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Xapi.Data.Queries
{
    [Obsolete("Class is deprecated, please use SEO module instead.", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
    public class SlugInfoQueryBuilder : QueryBuilder<SlugInfoQuery, SlugInfoResponse, SlugInfoResponseType>
    {
        public SlugInfoQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
            : base(mediator, authorizationService)
        {
        }

        protected override string Name => "slugInfo";
    }
}
