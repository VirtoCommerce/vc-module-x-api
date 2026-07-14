using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.Xapi.Core.Services;

/// <summary>
/// Resolves a store-aware public asset (image) URL for use in Experience API responses.
/// When the store defines <see cref="Store.AssetPublicUrl"/>, relative URLs are combined with it and
/// absolute URLs are rebased onto it; otherwise the URL is returned unchanged.
/// </summary>
public interface IStoreAssetPublicUrlResolver
{
    /// <param name="store">Store whose <see cref="Store.AssetPublicUrl"/> defines the asset domain.</param>
    /// <param name="url">Absolute or relative asset URL as stored/indexed. Returned as-is when null/empty,
    /// a data:/blob: URI, or when the store has no <see cref="Store.AssetPublicUrl"/> configured.</param>
    string GetAbsoluteUrl(Store store, string url);
}
