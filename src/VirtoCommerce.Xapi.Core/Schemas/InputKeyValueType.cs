using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class InputKeyValueType : ExtendableInputGraphType<KeyValue>
    {
        public InputKeyValueType()
        {
            Field(x => x.Key, nullable: false).Description("Dictionary key");
            Field(x => x.Value, nullable: true).Description("Dictionary value");
        }
    }
}
