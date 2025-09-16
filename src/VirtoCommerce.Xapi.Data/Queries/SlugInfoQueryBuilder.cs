using System.Threading.Tasks;
using GraphQL;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Queries;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Xapi.Data.Queries
{
    public class SlugInfoQueryBuilder : QueryBuilder<SlugInfoQuery, SlugInfoResponse, SlugInfoResponseType>
    {
        public SlugInfoQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
            : base(mediator, authorizationService)
        {
        }

        protected override string Name => "slugInfo";

        protected override Task BeforeMediatorSend(IResolveFieldContext<object> context, SlugInfoQuery request)
        {
            request.OrganizationId = context.GetCurrentOrganizationId();

            return base.BeforeMediatorSend(context, request);
        }
    }
}
