using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Xapi.Core.Schemas.ScalarTypes
{
    public sealed class OptionalNullableIntGraphType : IntGraphType
    {
        public OptionalNullableIntGraphType()
        {
            Name = "OptionalNullableInt";
        }

        public override object ParseValue(object value)
        {
            var parsedValue = base.ParseValue(value);

            var result = new Optional<int?>(parsedValue);

            return result;
        }
    }
}
