using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Schemas.ScalarTypes;

namespace VirtoCommerce.Xapi.Core.Schemas;

public class StoreDynamicPropertyValueType : ExtendableGraphType<StoreDynamicPropertyValue>
{
    public StoreDynamicPropertyValueType()
    {
        Field(x => x.Name, nullable: false).Description("Dynamic property name");
        Field<AnyValueGraphType>(nameof(StoreDynamicPropertyValue.Value)).Description("Dynamic property value");
    }
}
