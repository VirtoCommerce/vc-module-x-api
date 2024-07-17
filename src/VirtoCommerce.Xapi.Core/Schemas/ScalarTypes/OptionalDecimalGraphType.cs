using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Xapi.Core.Schemas.ScalarTypes
{
    public sealed class OptionalDecimalGraphType : DecimalGraphType
    {
        public OptionalDecimalGraphType()
        {
            Name = "OptionalDecimal";
        }

        public override object ParseValue(object value)
        {
            var parsedValue = base.ParseValue(value);

            var result = new Optional<decimal>(parsedValue);

            return result;
        }
    }
}
