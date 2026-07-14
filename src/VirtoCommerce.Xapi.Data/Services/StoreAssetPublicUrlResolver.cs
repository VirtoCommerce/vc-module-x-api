using System;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.Xapi.Core.Services;

namespace VirtoCommerce.Xapi.Data.Services;

/// <summary>
/// Default <see cref="IStoreAssetPublicUrlResolver"/>. Self-contained: when
/// <see cref="Store.AssetPublicUrl"/> is set, a relative URL is combined with it and an absolute URL is
/// rebased onto it (preserving path, query and fragment); otherwise the URL is returned unchanged.
/// Registered as a singleton.
/// </summary>
public class StoreAssetPublicUrlResolver : IStoreAssetPublicUrlResolver
{
    public virtual string GetAbsoluteUrl(Store store, string url)
    {
        ArgumentNullException.ThrowIfNull(store);

        // null/empty is returned as-is
        if (string.IsNullOrEmpty(url))
        {
            return url;
        }

        // No store override configured => unchanged behavior.
        var assetBaseUrl = store.AssetPublicUrl;
        if (string.IsNullOrEmpty(assetBaseUrl))
        {
            return url;
        }

        // data:/blob: are returned as-is
        if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        return IsAbsolute(url, out _)
            ? RebaseHost(url, assetBaseUrl) ?? url
            : CombineUrl(assetBaseUrl, url);
    }

    /// <summary>
    /// Combines the store asset base URL (a full URL or bare host, optionally with a base path)
    /// with a relative asset path.
    /// </summary>
    protected virtual string CombineUrl(string baseUrl, string relativeUrl)
    {
        return $"{NormalizeBaseUrl(baseUrl).TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
    }

    /// <summary>
    /// Replaces scheme/host/port of <paramref name="absoluteUrl"/> with those of
    /// <paramref name="baseUrl"/> (a full URL or bare host, optionally with a base path),
    /// preserving the original path, query and fragment. Returns null on parse failure.
    /// </summary>
    protected virtual string RebaseHost(string absoluteUrl, string baseUrl)
    {
        try
        {
            if (!IsAbsolute(absoluteUrl, out var source) ||
                !Uri.TryCreate(NormalizeBaseUrl(baseUrl), UriKind.Absolute, out var baseUri))
            {
                return null;
            }

            var basePath = baseUri.AbsolutePath.TrimEnd('/');
            var combinedPath = basePath + source.AbsolutePath;

            var builder = new UriBuilder
            {
                Scheme = baseUri.Scheme,
                Host = baseUri.Host,
                Port = baseUri.IsDefaultPort ? -1 : baseUri.Port,
                Path = combinedPath,
                Query = source.Query.TrimStart('?'),
                Fragment = source.Fragment.TrimStart('#'),
            };

            return builder.Uri.AbsoluteUri;
        }
        catch (UriFormatException)
        {
            return null;
        }
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        return baseUrl.Contains("://", StringComparison.Ordinal) ? baseUrl : $"https://{baseUrl}";
    }

    private static bool IsAbsolute(string value, out Uri uri)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
