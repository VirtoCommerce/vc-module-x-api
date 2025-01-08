using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Models.Facets;

namespace VirtoCommerce.Xapi.Core.Schemas.Facets
{
    public class RangeFacetResultType : ExtendableGraphType<RangeFacetResult>
    {
        public RangeFacetResultType()
        {
            Name = "RangeFacet";

            Field(d => d.Name, nullable: false).Description("The key/name  of the facet.");
            Field(d => d.Label, nullable: false).Description("Localized name of the facet.");
            Field<NonNullGraphType<FacetTypeEnum>>("FacetType")
                .Description("The three types of facets. Terms, Range, Filter");
            Field<NonNullGraphType<ListGraphType<NonNullGraphType<FacetRangeType>>>>("Ranges")
                .Description("Ranges")
                .Resolve(context => context.Source.Ranges);

            IsTypeOf = obj => obj is RangeFacetResult;
            Interface<FacetInterface>();
        }
    }
}
