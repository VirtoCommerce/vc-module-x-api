using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace VirtoCommerce.Xapi.Core.Binding
{
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, ImmutableArray<BoundProperty>> _boundPropertiesCache = new ConcurrentDictionary<Type, ImmutableArray<BoundProperty>>();

        public static IIndexModelBinder GetIndexModelBinder(this Type type, IIndexModelBinder defaultBinder)
        {
            var result = defaultBinder;
            var bindAttr = type.GetCustomAttributes<BindIndexFieldAttribute>().FirstOrDefault();

            if (bindAttr != null)
            {
                result = CreateBinder(bindAttr);
            }
            if (result != null)
            {
                result.BindingInfo = type.GetBindingInfo() ?? result.BindingInfo;
            }
            return result;
        }

        /// <summary>
        /// A binder configured for <paramref name="propInfo"/>, created fresh on every call so that the
        /// caller owns it outright — nothing else reads the returned instance, including its
        /// <see cref="IIndexModelBinder.BindingInfo"/>. Null when the property declares no binder.
        /// Prefer <see cref="GetBoundProperties"/>, which resolves a whole type once.
        /// </summary>
        public static IIndexModelBinder GetIndexModelBinder(this PropertyInfo propInfo)
        {
            return CreateBinder(propInfo);
        }

        /// <summary>
        /// The public instance properties of <paramref name="type"/> that carry a binder, each paired with
        /// its binder, cached against the type — pass the runtime type so an <c>AbstractTypeFactory</c>
        /// override is described by its own entry rather than by its base's.
        /// <para>Each paired binder is shared by every document bound through it, so a binder
        /// implementation must keep no per-call state — and a caller must not write to the binder or to
        /// its <see cref="IIndexModelBinder.BindingInfo"/>, which would retarget that property for every
        /// thread. Use <see cref="GetIndexModelBinder(PropertyInfo)"/> for an instance you own.</para>
        /// </summary>
        public static ImmutableArray<BoundProperty> GetBoundProperties(this Type type)
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

            if (result != null)
            {
                result.BindingInfo = GetBindingInfo(bindAttr) ?? result.BindingInfo;
            }

            return result;
        }

        private static IIndexModelBinder CreateBinder(BindIndexFieldAttribute attr)
        {
            var binderType = attr.BinderType ?? typeof(DefaultPropertyIndexBinder);

            return Activator.CreateInstance(binderType) as IIndexModelBinder;
        }

        private static BindingInfo GetBindingInfo(this Type type)
        {
            BindingInfo result = null;
            var bindAttr = type.GetCustomAttributes<BindIndexFieldAttribute>().FirstOrDefault();

            if (bindAttr != null)
            {
                result = GetBindingInfo(bindAttr);
            }
            return result;
        }

        private static BindingInfo GetBindingInfo(BindIndexFieldAttribute attr)
        {
            return new BindingInfo
            {
                FieldName = attr.FieldName
            };
        }
    }
}
