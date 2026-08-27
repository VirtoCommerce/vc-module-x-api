namespace VirtoCommerce.Xapi.Core.Models
{
    /// <summary>
    /// Base type for the ambient context objects X-module mappers take instead of positional
    /// parameters - the values ambient to the operation rather than distinct per mapper call.
    /// <list type="bullet">
    /// <item>Created only via <c>AbstractTypeFactory&lt;TDerived&gt;.TryCreateInstance()</c>, never
    /// <c>new</c>, so a consumer can substitute the type via <c>OverrideType</c>.</item>
    /// <item>Populated at the call site that owns the data, never on the mapper's interface, so a
    /// new datum is one assignment rather than a signature change. The call site may organize this
    /// as its own <c>protected virtual Create...Context(...)</c> hook; an override must enrich via
    /// <c>base.</c>, never rebuild from scratch.</item>
    /// <item>A derived context carries the operation's carrier object whole (e.g. a search
    /// response), not a projection of it, and inherits that carrier's cardinality - per-operation
    /// or per-item, whichever the carrier is. A derived context's own member is justified only for
    /// a call-site-local value not reachable from the carrier; a member that projects a carrier
    /// property is out.</item>
    /// <item><see cref="CultureName"/>/<see cref="CurrencyCode"/> are the module-agnostic ambient
    /// surface: populate them whenever the call site has the data, even with no in-module reader
    /// today. They carry the value in effect for the operation, never the value merely requested -
    /// where the call site normalizes (a store-language fallback, a currency resolution), pass the
    /// normalized result into the <c>Create...Context</c> hook explicitly rather than letting the
    /// hook re-derive it from the carrier; the base member and the raw request field are often both
    /// called <c>CultureName</c>, so the wrong assignment is the one that matches name-for-name. A
    /// mapper reads them only from these base members, never from the carrier's equivalent value,
    /// or a consumer's reassignment before calling <c>base.</c> is silently ignored.</item>
    /// <item>A consumer registers an override per derived context type - each closed
    /// <c>AbstractTypeFactory&lt;T&gt;</c> keeps its own registrations, not one for this base.</item>
    /// </list>
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
