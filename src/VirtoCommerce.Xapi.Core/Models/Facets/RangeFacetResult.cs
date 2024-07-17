using System.Collections.Generic;

namespace VirtoCommerce.Xapi.Core.Models.Facets
{
    public sealed class RangeFacetResult : FacetResult
    {
        public RangeFacetResult()
            : base(FacetTypes.Range)
        {
        }

        public IList<FacetRange> Ranges { get; set; } = new List<FacetRange>();
    }
}
