using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.Xapi.Core.Models;

/// <summary>
/// A frontend (Module Federation) plugin delivered by an installed module and
/// discovered by the platform for a given host app (e.g. <c>vc-frontend</c>).
/// XAPI-owned projection of the platform's <c>PluginDescriptor</c>.
/// </summary>
public class StorePlugin
{
    public string Id { get; set; }

    public string Version { get; set; }

    /// <summary>Permission the consuming SPA evaluates before loading the plugin. Null when unrestricted.</summary>
    public string Permission { get; set; }

    /// <summary>The plugin's entry asset (its <c>remoteEntry.js</c>).</summary>
    public StorePluginFile Entry { get; set; }

    /// <summary>Additional assets (extra scripts/styles) the plugin ships.</summary>
    public IList<StorePluginFile> ContentFiles { get; set; } = new List<StorePluginFile>();

    /// <summary>Module Federation remote coordinates.</summary>
    public StorePluginRemote Remote { get; set; }

    /// <summary>
    /// Maps a platform <see cref="PluginDescriptor"/> to its XAPI projection.
    /// Pure and null-tolerant so it can be unit-tested without a running schema.
    /// </summary>
    public static StorePlugin FromDescriptor(PluginDescriptor descriptor)
    {
        if (descriptor == null)
        {
            return null;
        }

        return new StorePlugin
        {
            Id = descriptor.Id,
            Version = descriptor.Version,
            Permission = descriptor.Permission,
            Entry = FromFileDescriptor(descriptor.Entry),
            ContentFiles = descriptor.ContentFiles?.Select(FromFileDescriptor).ToList() ?? [],
            Remote = descriptor.Remote == null
                ? null
                : new StorePluginRemote
                {
                    Name = descriptor.Remote.Name,
                    Exposed = descriptor.Remote.Exposed,
                },
        };
    }

    private static StorePluginFile FromFileDescriptor(ContentFileDescriptor file)
    {
        return file == null
            ? null
            : new StorePluginFile
            {
                Type = file.Type,
                Path = file.Path,
                Hash = file.Hash,
            };
    }
}
