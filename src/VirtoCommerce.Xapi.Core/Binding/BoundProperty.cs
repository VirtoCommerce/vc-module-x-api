using System.Reflection;

namespace VirtoCommerce.Xapi.Core.Binding
{
    /// <summary>
    /// A property that carries an index model binder, paired with the binder resolved for it.
    /// <see cref="Binder"/> is never null.
    /// </summary>
    public readonly struct BoundProperty(PropertyInfo property, IIndexModelBinder binder)
    {
        public PropertyInfo Property { get; } = property;

        public IIndexModelBinder Binder { get; } = binder;
    }
}
