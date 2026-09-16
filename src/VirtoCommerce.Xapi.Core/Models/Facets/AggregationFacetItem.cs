using System.Collections.Generic;

namespace VirtoCommerce.Xapi.Core.Models.Facets;

/// <summary>
/// Module-agnostic mirror of each domain's own <c>AggregationItem</c>-shaped type.
/// </summary>
public class AggregationFacetItem
{
    public object Value { get; set; }

    public int Count { get; set; }

    public bool IsApplied { get; set; }

    public IList<AggregationFacetLabel> Labels { get; set; }

    public string RequestedLowerBound { get; set; }

    public string RequestedUpperBound { get; set; }

    public bool IncludeLower { get; set; }

    public bool IncludeUpper { get; set; }
}
