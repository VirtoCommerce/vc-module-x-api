namespace VirtoCommerce.Xapi.Core.Models
{
    /// <summary>
    /// Base type for the ambient context objects X-module mappers take instead of positional
    /// parameters. A member belongs here only if it is the same for every item in the operation
    /// (e.g. culture, currency); per-item values derived from the caller's own iteration (e.g. an
    /// ordinal) do not belong on a context and should stay as separate arguments.
    /// Built via <c>AbstractTypeFactory&lt;T&gt;.TryCreateInstance()</c> so a consumer can register a
    /// derived type with extra ambient fields; populate it through a <c>protected virtual</c> creation
    /// hook on the mapper, not inline at each call site, so overriding it is a one-method change.
    /// </summary>
    public class MappingContext
    {
        public string CultureName { get; set; }
    }
}
