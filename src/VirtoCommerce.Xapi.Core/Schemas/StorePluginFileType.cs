using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Schemas;

public class StorePluginFileType : ExtendableGraphType<StorePluginFile>
{
    public StorePluginFileType()
    {
        Field(x => x.Type, nullable: true).Description("Asset kind, e.g. 'script' or 'style'");
        Field(x => x.Path, nullable: true).Description("Public URL of the asset");
        Field(x => x.Hash, nullable: true).Description("Cache-busting hash derived from the file's last-write time");
    }
}
