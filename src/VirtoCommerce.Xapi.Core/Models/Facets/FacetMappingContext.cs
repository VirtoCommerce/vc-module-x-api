namespace VirtoCommerce.Xapi.Core.Models.Facets
{
    /// <summary>
    /// Shared <see cref="MappingContext"/> override axis for every <c>ToFacetResult</c> mapper
    /// (x-catalog, x-order, x-pickup). Empty on purpose - all it needs is the inherited
    /// <see cref="MappingContext.CultureName"/>.
    /// </summary>
    public class FacetMappingContext : MappingContext
    {
    }
}
