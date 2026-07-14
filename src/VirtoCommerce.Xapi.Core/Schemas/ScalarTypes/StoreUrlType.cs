using GraphQL.Types;

namespace VirtoCommerce.Xapi.Core.Schemas.ScalarTypes
{
    /// <summary>
    /// String scalar for asset (image) URL fields. Fields declared with this type are automatically
    /// resolved against the store's asset public URL by <see cref="Infrastructure.StoreUrlSchemaVisitor"/> —
    /// no per-field resolver is required.
    /// </summary>
    public sealed class StoreUrlType : StringGraphType
    {
        public StoreUrlType()
        {
            Name = "StoreAssetUrl";
            Description = "Asset URL string. When the store defines an asset public URL, the value is resolved against it.";
        }
    }
}
