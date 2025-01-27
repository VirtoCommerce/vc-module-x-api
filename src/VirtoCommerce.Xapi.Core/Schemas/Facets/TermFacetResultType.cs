using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Models.Facets;

namespace VirtoCommerce.Xapi.Core.Schemas.Facets
{
    public class TermFacetResultType : ExtendableGraphType<TermFacetResult>
    {
        public TermFacetResultType()
        {
            Name = "TermFacet";

            Field(d => d.Name, nullable: false).Description("The key/name  of the facet.");
            Field(d => d.Label, nullable: false).Description("Localized name of the facet.");
            Field<NonNullGraphType<FacetTypeEnum>>("FacetType")
                .Description("Three facet types: Terms, Range, and Filter");
            Field<NonNullGraphType<ListGraphType<NonNullGraphType<FacetTermType>>>>("Terms")
                .Description("Terms")
                .Resolve(context => context.Source.Terms);

            IsTypeOf = obj => obj is TermFacetResult;
            Interface<FacetInterface>();
        }
    }
}
