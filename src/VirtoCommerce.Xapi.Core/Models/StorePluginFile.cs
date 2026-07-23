namespace VirtoCommerce.Xapi.Core.Models;

/// <summary>
/// A single asset (script or style) that belongs to a frontend plugin.
/// Mirrors the platform's <c>ContentFileDescriptor</c>.
/// </summary>
public class StorePluginFile
{
    /// <summary>Asset kind, e.g. <c>script</c> or <c>style</c>.</summary>
    public string Type { get; set; }

    /// <summary>Public URL of the asset, e.g. <c>/modules/$(ModuleId)/plugins/vc-frontend/remoteEntry.js</c>.</summary>
    public string Path { get; set; }

    /// <summary>Cache-busting hash derived from the file's last-write time.</summary>
    public string Hash { get; set; }
}
