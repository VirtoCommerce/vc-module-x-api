namespace VirtoCommerce.Xapi.Core.Models;

public class ModuleSettings
{
    public string ModuleId { get; set; }

    public string Version { get; set; }

    public ModuleSetting[] Settings { get; set; }
}
