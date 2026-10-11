using System.Collections.Generic;

namespace VirtoCommerce.Xapi.Core.Models.Facets;

/// <summary>
/// Module-agnostic mirror of each domain's own <c>Aggregation</c>-shaped type - each is a distinct
/// CLR type, so a caller adapts its own into this one before calling <see cref="Services.IFacetMapper"/>.
/// A field a caller's type lacks (e.g. orders has no <see cref="Statistics"/>) is left at its default.
/// </summary>
public class AggregationFacetSource
{
    public string AggregationType { get; set; }

    public string Field { get; set; }

    public IList<AggregationFacetLabel> Labels { get; set; }

    public IList<AggregationFacetItem> Items { get; set; }

    public AggregationFacetStatistics Statistics { get; set; }

    public string TermValuesSortingType { get; set; }
}
