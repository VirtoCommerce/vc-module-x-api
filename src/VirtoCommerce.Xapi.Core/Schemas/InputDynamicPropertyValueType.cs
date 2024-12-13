using GraphQL.Types;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Schemas.ScalarTypes;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class InputDynamicPropertyValueType : ExtendableInputGraphType<DynamicPropertyValue>
    {
        public InputDynamicPropertyValueType()
        {
            Field(x => x.Name).Description("Dynamic property name");
            Field<DynamicPropertyValueGraphType>(nameof(DynamicPropertyObjectValue.Value),
                "Dynamic property value. ID must be passed for dictionary item");
            Field<StringGraphType>("locale", resolve: x => x.Source.Locale, description: "Language (\"en-US\") for multilingual property", deprecationReason: "Deprecated. Use cultureName field. Will be removed in v. 1.50+");
            Field("cultureName", x => x.Locale, true).Description("Culture name (\"en-US\") for multilingual property");
        }
    }
}
