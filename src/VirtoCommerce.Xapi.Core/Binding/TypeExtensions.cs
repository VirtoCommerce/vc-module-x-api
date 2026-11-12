using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.Xapi.Core.Binding;

public static class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, ImmutableArray<BoundProperty>> _boundPropertiesCache = new();

    public static IIndexModelBinder GetIndexModelBinder(this Type type, IIndexModelBinder defaultBinder)
    {
        var result = defaultBinder;
        var bindAttr = type.GetCustomAttributes<BindIndexFieldAttribute>().FirstOrDefault();

        if (bindAttr != null)
        {
            result = CreateBinder(bindAttr);
        }

        result?.BindingInfo = type.GetBindingInfo() ?? result.BindingInfo;

        return result;
    }

    /// <summary>
    /// A binder configured for <paramref name="propInfo"/>, created fresh on every call so that the
    /// caller owns it outright — nothing else reads the returned instance, including its
    /// <see cref="IIndexModelBinder.BindingInfo"/>. Null when the property carries no <see cref="BindIndexFieldAttribute"/>.
    /// To bind every indexed property of an object, use <see cref="BindIndexedProperties"/>, which resolves a type once.
    /// </summary>
    public static IIndexModelBinder GetIndexModelBinder(this PropertyInfo propInfo)
    {
        return CreateBinder(propInfo);
    }

    /// <summary>
    /// Sets every public instance property of <paramref name="target"/> that declares a
    /// <see cref="BindIndexFieldAttribute"/> to the value its binder reads from <paramref name="searchDocument"/>.
    /// The binders are resolved once per runtime type of <paramref name="target"/> and reused by every call,
    /// so a binder implementation must keep no per-call state; they are never handed out to callers.
    /// The field name is read from the attribute once per type.
    /// </summary>
    public static void BindIndexedProperties(this object target, SearchDocument searchDocument)
    {
        ArgumentNullException.ThrowIfNull(target);

        foreach (var boundProperty in GetBoundProperties(target.GetType()))
        {
            boundProperty.Property.SetValue(target, boundProperty.Binder.BindModel(searchDocument));
        }
    }

    private static ImmutableArray<BoundProperty> GetBoundProperties(Type type)
    {
        return _boundPropertiesCache.GetOrAdd(type, x => x
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(propInfo => new BoundProperty(propInfo, propInfo.GetIndexModelBinder()))
            .Where(boundProperty => boundProperty.Binder != null)
            .ToImmutableArray());
    }

    private static IIndexModelBinder CreateBinder(PropertyInfo propInfo)
    {
        var bindAttr = propInfo.GetCustomAttributes<BindIndexFieldAttribute>().FirstOrDefault();
        if (bindAttr == null)
        {
            return null;
        }

        var result = CreateBinder(bindAttr);

        result?.BindingInfo = GetBindingInfo(bindAttr) ?? result.BindingInfo;

        return result;
    }

    private static IIndexModelBinder CreateBinder(BindIndexFieldAttribute attr)
    {
        var binderType = attr.BinderType ?? typeof(DefaultPropertyIndexBinder);

        return Activator.CreateInstance(binderType) as IIndexModelBinder;
    }

    private static BindingInfo GetBindingInfo(this Type type)
    {
        var bindAttr = type.GetCustomAttributes<BindIndexFieldAttribute>().FirstOrDefault();

        return bindAttr != null ? GetBindingInfo(bindAttr) : null;
    }

    private static BindingInfo GetBindingInfo(BindIndexFieldAttribute attr)
    {
        return new BindingInfo
        {
            FieldName = attr.FieldName
        };
    }
}
