namespace VirtoCommerce.Xapi.Core.Models.Facets;

/// <summary>
/// Module-agnostic mirror of each domain's own <c>AggregationLabel</c>-shaped type.
/// </summary>
public class AggregationFacetLabel
{
    public string Language { get; set; }

    public string Label { get; set; }
}
