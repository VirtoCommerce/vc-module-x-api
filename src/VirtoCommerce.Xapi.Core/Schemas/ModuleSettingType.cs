using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Schemas.ScalarTypes;

namespace VirtoCommerce.Xapi.Core.Schemas;

public class ModuleSettingType : ObjectGraphType<ModuleSetting>
{
    public ModuleSettingType()
    {
        Field(x => x.Name, nullable: false);
        Field<ModuleSettingValueGraphType>("value", resolve: x => x.Source.Value);
    }
}
