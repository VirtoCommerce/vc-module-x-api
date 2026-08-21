namespace VirtoCommerce.Xapi.Core.Models.Facets
{
    /// <summary>
    /// The <see cref="MappingContext"/> override axis shared by every <c>ToFacetResult</c>-style
    /// mapper (x-catalog, x-order, x-pickup): a common type each of them can register a further
    /// derivative of, rather than each declaring its own unrelated facet context. It carries only
    /// what is ambient for every aggregation in one call (currently just
    /// <see cref="MappingContext.CultureName"/>, inherited); a per-facet ordinal is not ambient and
    /// stays a separate method argument. It is otherwise empty on purpose, not unfinished: the only
    /// facet-shaped candidate, <c>TermValuesSortingType</c>, comes off <c>Aggregation</c> itself, so
    /// it is source data being mapped, not context describing the mapping.
    /// </summary>
    public class FacetMappingContext : MappingContext
    {
    }
}
