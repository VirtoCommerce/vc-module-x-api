using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Xapi.Core.Helpers
{
    public static class GenericTypeHelper
    {
        public static Type GetActualType<T>()
        {
            var type = typeof(T);
            var result = AbstractTypeFactory<T>.FindTypeInfoByName(type.Name)?.Type;
            return result ?? type;
        }
    }
}
