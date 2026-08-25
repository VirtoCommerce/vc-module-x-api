using VirtoCommerce.Xapi.Core.Models.Facets;

namespace VirtoCommerce.Xapi.Core.Services;

/// <summary>
/// Single <c>Aggregation -&gt; FacetResult</c> conversion shared by every search-indexed X-module,
/// each of which adapts its own native aggregation type into <see cref="AggregationFacetSource"/> first.
/// </summary>
public interface IFacetMapper
{
    /// <returns>Null if <paramref name="source"/> is null, or if its <c>AggregationType</c> is not recognized.</returns>
    FacetResult ToFacetResult(AggregationFacetSource source, FacetMappingContext context);

    /// <summary>Single construction point for the context <see cref="ToFacetResult"/> takes.</summary>
    FacetMappingContext CreateFacetMappingContext(string cultureName);
}
