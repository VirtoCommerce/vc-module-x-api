using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Schemas.ScalarTypes;

namespace VirtoCommerce.Xapi.Core.Schemas;

public class ModuleSettingType : ExtendableGraphType<ModuleSetting>
{
    public ModuleSettingType()
    {
        Field(x => x.Name, nullable: false);
        Field<ModuleSettingValueGraphType>("value").Resolve(x => x.Source.Value);
    }
}
