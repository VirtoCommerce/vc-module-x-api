using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Xapi.Core.Schemas.ScalarTypes
{
    public sealed class OptionalNullableBooleanGraphType : BooleanGraphType
    {
        public OptionalNullableBooleanGraphType()
        {
            Name = "OptionalNullableBoolean";
        }

        public override object ParseValue(object value)
        {
            var parsedValue = base.ParseValue(value);

            var result = new Optional<bool?>(parsedValue);

            return result;
        }
    }
}
