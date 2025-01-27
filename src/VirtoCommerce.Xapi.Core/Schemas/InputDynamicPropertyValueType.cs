using GraphQL.Types;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Schemas.ScalarTypes;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class InputDynamicPropertyValueType : InputObjectGraphType<DynamicPropertyValue>
    {
        public InputDynamicPropertyValueType()
        {
            Field(x => x.Name).Description("Dynamic property name");
            Field<DynamicPropertyValueGraphType>(nameof(DynamicPropertyObjectValue.Value)).Description("Dynamic property value. ID must be passed for dictionary item");
            Field("cultureName", x => x.Locale, true).Description("Culture name (\"en-US\") for multilingual property");
        }
    }
}
