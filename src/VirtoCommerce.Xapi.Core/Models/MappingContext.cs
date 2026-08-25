namespace VirtoCommerce.Xapi.Core.Models
{
    /// <summary>
    /// Base type for the ambient context objects X-module mappers take instead of positional
    /// parameters. A member belongs here only if it is the same for every item in the operation
    /// (e.g. culture, currency) - per-item values (e.g. an ordinal) stay separate arguments.
    /// Built via <c>AbstractTypeFactory&lt;T&gt;.TryCreateInstance()</c>; populate it through one
    /// creation hook on the mapper that owns the ambient values (a <c>protected virtual</c> method,
    /// or - for a context shared by several modules - one interface method, e.g.
    /// <c>IFacetMapper.CreateFacetMappingContext</c>), never inline at each call site. That hook must
    /// set every base member its mappers read - an inherited member it leaves unset is silently inert.
    /// A consumer registers an override per derived context type, not once for the base: each closed
    /// <c>AbstractTypeFactory&lt;T&gt;</c> keeps its own registrations.
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
