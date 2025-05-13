using System;
using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Queries
{
    [Obsolete("Class is deprecated, please use SEO module instead.", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
    public class SlugInfoQuery : Query<SlugInfoResponse>
    {
        public string Permalink { get; set; }
        public string StoreId { get; set; }
        public string UserId { get; set; }
        public string CultureName { get; set; }

        public override IEnumerable<QueryArgument> GetArguments()
        {
            yield return Argument<StringGraphType>(nameof(Permalink));
            yield return Argument<StringGraphType>(nameof(StoreId));
            yield return Argument<StringGraphType>(nameof(UserId));
            yield return Argument<StringGraphType>(nameof(CultureName));
        }

        public override void Map(IResolveFieldContext context)
        {
            Permalink = context.GetArgument<string>(nameof(Permalink));
            StoreId = context.GetArgument<string>(nameof(StoreId));
            UserId = context.GetCurrentUserId();
            CultureName = context.GetArgument<string>(nameof(CultureName));
        }
    }
}
