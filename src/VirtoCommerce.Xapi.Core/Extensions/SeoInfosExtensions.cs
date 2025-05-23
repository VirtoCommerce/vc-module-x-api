using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Xapi.Core.Extensions;

public static class SeoInfosExtensions
{
    [Obsolete("Use VirtoCommerce.StoreModule.Core.Extensions", DiagnosticId = "VC0010", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
    public static SeoInfo GetBestMatchingSeoInfo(this IList<SeoInfo> seoInfos, string storeId, string cultureName)
    {
        return GetBestMatchingSeoInfoInternal(seoInfos, storeId, cultureName, cultureName, slug: null, permalink: null);
    }

    [Obsolete("Use VirtoCommerce.StoreModule.Core.Extensions", DiagnosticId = "VC0010", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
    public static SeoInfo GetBestMatchingSeoInfo(this IList<SeoInfo> seoInfos, string storeId, string cultureName, string slug)
    {
        return GetBestMatchingSeoInfoInternal(seoInfos, storeId, cultureName, cultureName, slug, permalink: null);
    }

    [Obsolete("Use VirtoCommerce.StoreModule.Core.Extensions", DiagnosticId = "VC0010", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
    public static SeoInfo GetBestMatchingSeoInfo(this IList<SeoInfo> seoInfos, string storeId, string defaultStoreLang, string cultureName, string slug)
    {
        return GetBestMatchingSeoInfoInternal(seoInfos, storeId, defaultStoreLang, cultureName, slug, permalink: null);
    }

    [Obsolete("Use VirtoCommerce.StoreModule.Core.Extensions", DiagnosticId = "VC0010", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
    public static SeoInfo GetBestMatchingSeoInfo(this IList<SeoInfo> seoInfos, string storeId, string defaultStoreLang, string cultureName, string slug, string permalink)
    {
        return GetBestMatchingSeoInfoInternal(seoInfos, storeId, defaultStoreLang, cultureName, slug, permalink);
    }

    [Obsolete("Use VirtoCommerce.StoreModule.Core.Extensions", DiagnosticId = "VC0010", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
    public static SeoInfo GetBestMatchingSeoInfo(this IList<SeoInfo> seoInfos, string defaultStoreLang,
        SeoSearchCriteria criteria)
    {
        return GetBestMatchingSeoInfoInternal(seoInfos, criteria.StoreId, defaultStoreLang, criteria.LanguageCode,
            criteria.Slug, criteria.Permalink);
    }

    private static SeoInfo GetBestMatchingSeoInfoInternal(IList<SeoInfo> seoInfos, string storeId,
        string defaultStoreLang, string cultureName, string slug, string permalink)
    {
        if (storeId.IsNullOrEmpty() || cultureName.IsNullOrEmpty())
        {
            return null;
        }
        return seoInfos.GetBestMatchingSeoInfos(storeId, defaultStoreLang, cultureName, slug, permalink);
    }

    public static SeoInfo GetFallbackSeoInfo(string id, string name, string cultureName)
    {
        var result = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
        result.SemanticUrl = id;
        result.LanguageCode = cultureName;
        result.Name = name;
        return result;
    }

    private static SeoInfo GetBestMatchingSeoInfos(this IEnumerable<SeoInfo> seoRecords, string storeId, string defaultStoreLang, string language, string slug, string permalink)
    {
        var result = seoRecords?.Select(s => new
        {
            SeoRecord = s,
            Score = CalculateScore(s, slug, permalink, storeId, defaultStoreLang, language)
        })
        .OrderByDescending(x => x.Score)
        .Select(x => x.SeoRecord)
        .FirstOrDefault();

        return result;
    }

    private static int CalculateScore(SeoInfo seoInfo, string slug, string permalink, string storeId, string defaultStoreLang, string language)
    {
        var score = new[]
        {
                seoInfo.IsActive,
                !string.IsNullOrEmpty(permalink) && permalink.TrimStart('/').EqualsIgnoreCase(seoInfo.SemanticUrl.TrimStart('/')),
                !string.IsNullOrEmpty(slug) && slug.TrimStart('/').EqualsIgnoreCase(seoInfo.SemanticUrl.TrimStart('/')),
                storeId.EqualsIgnoreCase(seoInfo.StoreId),
                language.Equals(seoInfo.LanguageCode),
                defaultStoreLang.EqualsIgnoreCase(seoInfo.LanguageCode),
                seoInfo.LanguageCode.IsNullOrEmpty()
            }
        .Reverse()
        .Select((valid, index) => valid ? 1 << index : 0)
        .Sum();

        return score;
    }
}
