namespace VirtoCommerce.Xapi.Core.Models.Facets
{
    /// <summary>
    /// Ambient context for <c>ToFacetResult</c>-style mapping methods. Carries only what is the same
    /// for every aggregation being mapped in one call (e.g. <see cref="MappingContext.CultureName"/>);
    /// a per-facet ordinal is not ambient and is a separate method argument instead.
    /// </summary>
    public class FacetMappingContext : MappingContext
    {
    }
}
