using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class KeyValueType : ExtendableGraphType<KeyValue>
    {
        public KeyValueType()
        {
            Field(x => x.Key, nullable: false).Description("Dictionary key");
            Field(x => x.Value, nullable: true).Description("Dictionary value");
        }
    }
}
