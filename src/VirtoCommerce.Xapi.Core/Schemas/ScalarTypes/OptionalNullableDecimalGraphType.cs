using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Xapi.Core.Schemas.ScalarTypes
{
    public sealed class OptionalNullableDecimalGraphType : DecimalGraphType
    {
        public OptionalNullableDecimalGraphType()
        {
            Name = "OptionalNullableDecimal";
        }

        public override object ParseValue(object value)
        {
            var parsedValue = base.ParseValue(value);

            var result = new Optional<decimal?>(parsedValue);

            return result;
        }
    }
}
