using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Models.Facets;

namespace VirtoCommerce.Xapi.Core.Schemas.Facets
{
    public class FacetInterface : InterfaceGraphType<FacetResult>
    {
        public FacetInterface()
        {
            Name = "Facet";

            Field(d => d.Name, nullable: false).Description("The key/name  of the facet.");
            Field(d => d.Label, nullable: false).Description("Localized name of the facet.");
            Field<NonNullGraphType<FacetTypeEnum>>("FacetType").Description("Three facet types: Terms, Range, and Filter");
        }
    }
}
