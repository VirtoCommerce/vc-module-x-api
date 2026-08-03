using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Schemas;

public class StorePluginRemoteType : ExtendableGraphType<StorePluginRemote>
{
    public StorePluginRemoteType()
    {
        Field(x => x.Name, nullable: true).Description("Module Federation remote name");
        Field(x => x.Exposed, nullable: true).Description("Exposed module path, e.g. './Module'");
    }
}
