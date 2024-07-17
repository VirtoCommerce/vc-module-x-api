using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Xapi.Core.Schemas.ScalarTypes
{
    public sealed class OptionalStringGraphType : StringGraphType
    {
        public OptionalStringGraphType()
        {
            Name = "OptionalString";
        }

        public override object ParseValue(object value)
        {
            var parsedValue = base.ParseValue(value);

            var result = new Optional<string>(parsedValue);

            return result;
        }
    }
}
