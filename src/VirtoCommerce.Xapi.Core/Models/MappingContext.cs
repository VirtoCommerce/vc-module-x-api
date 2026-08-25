namespace VirtoCommerce.Xapi.Core.Models
{
    /// <summary>
    /// Base type for the ambient context objects X-module mappers take instead of positional
    /// parameters. A member belongs here only if it is the same for every item in the operation
    /// (e.g. culture, currency) - per-item values (e.g. an ordinal) stay separate arguments.
    /// Built via <c>AbstractTypeFactory&lt;T&gt;.TryCreateInstance()</c> so a consumer can register a
    /// derived type; populate it through a single creation hook (a <c>protected virtual</c> method,
    /// or - for a context shared by several modules - one interface method on the shared mapper,
    /// e.g. <c>IFacetMapper.CreateFacetMappingContext</c>), not inline at each call site.
    /// </summary>
    public abstract class MappingContext
    {
        public string CultureName { get; set; }

        /// <summary>
        /// The unresolved currency code; a resolved <c>Currency</c> belongs on a derived context instead.
        /// </summary>
        public string CurrencyCode { get; set; }
    }
}
