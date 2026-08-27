namespace VirtoCommerce.Xapi.Core.Models.Facets
{
    /// <summary>
    /// Base type <c>ToFacetResult</c> takes. Each consuming module (x-catalog, x-order, x-pickup)
    /// derives its own empty subtype (e.g. <c>CatalogFacetMappingContext</c>) and registers
    /// overrides against that closed type - never against this base directly. Nothing registers
    /// this base with <c>AbstractTypeFactory</c>, so two modules overriding it here would each
    /// append rather than replace one another; <c>TryCreateInstance()</c> would then resolve
    /// through the inheritance scan to whichever module's <c>Initialize</c> ran first for the
    /// whole process, silently dropping the other's override.
    /// </summary>
    public class FacetMappingContext : MappingContext
    {
    }
}
