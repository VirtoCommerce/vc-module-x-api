namespace VirtoCommerce.Xapi.Core.Models
{
    /// <summary>
    /// Base type for the ambient context objects X-module mappers take instead of positional
    /// parameters. A member belongs here only if it is the same for every item in the operation
    /// (e.g. culture, currency); per-item values stay separate arguments.
    /// <list type="bullet">
    /// <item>Created only via <c>AbstractTypeFactory&lt;TDerived&gt;.TryCreateInstance()</c>, never
    /// <c>new</c>, so a consumer can substitute the type via <c>OverrideType</c>.</item>
    /// <item>Populated at the call site that owns the data, never on the mapper's interface, so a
    /// new datum is one assignment rather than a signature change. The call site may organize this
    /// as its own <c>protected virtual Create...Context(...)</c> hook; an override must enrich via
    /// <c>base.</c>, never rebuild from scratch.</item>
    /// <item>A derived context carries the operation's carrier object whole (e.g. a search
    /// response), not a projection of it, and inherits that carrier's cardinality.</item>
    /// <item><see cref="CultureName"/>/<see cref="CurrencyCode"/> are the module-agnostic ambient
    /// surface: populate them whenever the call site has the data, even with no in-module reader
    /// today. A mapper reads them only from these base members, never from a derived context's
    /// equivalent field, or a consumer's reassignment is silently ignored.</item>
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
