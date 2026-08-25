namespace VirtoCommerce.Xapi.Core.Models.Facets;

/// <summary>
/// Module-agnostic mirror of each domain's own <c>AggregationStatistics</c>-shaped type.
/// </summary>
public class AggregationFacetStatistics
{
    public double? Min { get; set; }

    public double? Max { get; set; }
}
