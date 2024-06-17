using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Schemas;

public class ModuleSettingsType : ObjectGraphType<ModuleSettings>
{
    public ModuleSettingsType()
    {
        Field(x => x.ModuleId, nullable: false);
        Field<NonNullGraphType<ListGraphType<NonNullGraphType<ModuleSettingType>>>>("settings");
    }
}
