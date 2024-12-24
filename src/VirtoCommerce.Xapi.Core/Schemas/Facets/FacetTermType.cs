using VirtoCommerce.Xapi.Core.Models.Facets;

namespace VirtoCommerce.Xapi.Core.Schemas.Facets
{
    public class FacetTermType : ExtendableGraphType<FacetTerm>
    {
        public FacetTermType()
        {
            Field(d => d.Term, nullable: false).Description("term");
            Field(d => d.Count, nullable: false).Description("count");
            Field(d => d.IsSelected, nullable: false).Description("is selected state");
            Field(d => d.Label, nullable: false);
        }
    }
}
