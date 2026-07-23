using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Schemas;

public class StorePluginType : ExtendableGraphType<StorePlugin>
{
    public StorePluginType()
    {
        Field(x => x.Id, nullable: false).Description("Plugin ID");
        Field(x => x.Version, nullable: true).Description("Plugin version");
        Field(x => x.Permission, nullable: true).Description("Permission the consuming SPA evaluates before loading the plugin");
        Field<StorePluginFileType>(nameof(StorePlugin.Entry)).Description("The plugin's entry asset (its remoteEntry.js)").Resolve(context => context.Source.Entry);
        Field<NonNullGraphType<ListGraphType<NonNullGraphType<StorePluginFileType>>>>(nameof(StorePlugin.ContentFiles)).Description("Additional assets the plugin ships").Resolve(context => context.Source.ContentFiles);
        Field<StorePluginRemoteType>(nameof(StorePlugin.Remote)).Description("Module Federation remote coordinates").Resolve(context => context.Source.Remote);
    }
}
