using System;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    public sealed class Optional<T>
    {
        public Optional()
        {
        }

        public Optional(T value, bool isSpecified)
        {
            Value = value;
            IsSpecified = isSpecified;
        }

        public Optional(object value)
        {
            IsSpecified = true;

            if (value != null)
            {
                Value = (T)value;
            }
        }

        public T Value { get; set; }

        public bool IsSpecified { get; set; }
    }

    public static class Optional
    {
        public static void SetValue<T>(Optional<T> optional, Action<T> setValue)
        {
            if (optional?.IsSpecified == true)
            {
                setValue(optional.Value);
            }
        }
    }
}
