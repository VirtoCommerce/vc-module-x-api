using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Queries;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Xapi.Data.Queries
{
    public class GetStoreQueryBuilder : QueryBuilder<GetStoreQuery, StoreResponse, StoreResponseType>
    {
        protected override string Name => "store";

        public GetStoreQueryBuilder(IAuthorizationService authorizationService)
            : base(authorizationService)
        {
        }

        [Obsolete("Use the constructor without IMediator. The mediator is resolved from context.RequestServices per request.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public GetStoreQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
            : this(authorizationService)
        {
        }
    }
}
