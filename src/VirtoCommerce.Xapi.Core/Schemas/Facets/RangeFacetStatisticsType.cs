using VirtoCommerce.Xapi.Core.Models.Facets;

namespace VirtoCommerce.Xapi.Core.Schemas.Facets
{
    public class RangeFacetStatisticsType : ExtendableGraphType<RangeFacetStatistics>
    {
        public RangeFacetStatisticsType()
        {
            Name = "RangeFacetStatistics";

            Field(d => d.Max, nullable: true).Description("The maximum value in the range or across ranges.");
            Field(d => d.Min, nullable: true).Description("The minimum value in the range or across ranges.");
        }
    }
}
