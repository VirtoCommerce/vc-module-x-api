namespace VirtoCommerce.Xapi.Core.Models;

/// <summary>
/// Module Federation remote coordinates for a frontend plugin.
/// Mirrors the platform's <c>PluginRemoteDescriptor</c>.
/// </summary>
public class StorePluginRemote
{
    /// <summary>Federation remote name.</summary>
    public string Name { get; set; }

    /// <summary>Exposed module path, e.g. <c>./Module</c>.</summary>
    public string Exposed { get; set; }
}
