using GraphQL.Types;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class DynamicPropertyValueTypeEnum : EnumerationGraphType<Platform.Core.DynamicProperties.DynamicPropertyValueType>
    {
        public DynamicPropertyValueTypeEnum()
        {
            Name = "DynamicPropertyValueTypes";
            Description = "Dynamic property value type";
        }
    }
}
