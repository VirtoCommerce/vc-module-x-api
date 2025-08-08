using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Seo.Core.Models;
using VirtoCommerce.Seo.Core.Services;
using VirtoCommerce.StoreModule.Core.Extensions;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Queries;

namespace VirtoCommerce.Xapi.Data.Queries;

public class SlugInfoQueryHandler(
    ICompositeSeoResolver seoResolver,
    IStoreService storeService,
    IBrokenLinkSearchService brokenLinkSearchService,
    IRedirectResolver redirectResolver)
    : IQueryHandler<SlugInfoQuery, SlugInfoResponse>
{

    public async Task<SlugInfoResponse> Handle(SlugInfoQuery request, CancellationToken cancellationToken)
    {
        var result = new SlugInfoResponse();

        if (string.IsNullOrEmpty(request.Permalink))
        {
            return result;
        }

        var store = await storeService.GetByIdAsync(request.StoreId);
        if (store is null)
        {
            return result;
        }

        var redirectResult = await redirectResolver.ResolveRedirect(request.StoreId, request.Permalink);
        if (redirectResult != null)
        {
            result.RedirectUrl = redirectResult;
            return result;
        }

        var currentCulture = request.CultureName ?? store.DefaultLanguage;

        var segments = request.Permalink.Split("/", StringSplitOptions.RemoveEmptyEntries);
        var lastSegment = segments.LastOrDefault();

        var criteria = AbstractTypeFactory<SeoSearchCriteria>.TryCreateInstance();
        criteria.StoreId = store.Id;
        criteria.LanguageCode = currentCulture;
        criteria.Permalink = request.Permalink;
        criteria.Slug = lastSegment;
        criteria.UserId = request.UserId;

        result.EntityInfo = await GetBestMatchingSeoInfo(criteria, store);

        if (result.EntityInfo == null)
        {
            var brokenLinkCriteria = AbstractTypeFactory<BrokenLinkSearchCriteria>.TryCreateInstance();

            brokenLinkCriteria.Permalink = request.Permalink;
            brokenLinkCriteria.StoreId = store.Id;
            brokenLinkCriteria.Status = Seo.Core.ModuleConstants.LinkStatus.Resolved;
            brokenLinkCriteria.LanguageCode = request.CultureName;

            var brokenLinkResult = await brokenLinkSearchService.SearchAsync(brokenLinkCriteria);

            if (brokenLinkResult.Results.Count > 0)
            {
                var resultItem = brokenLinkResult.Results.FirstOrDefault(x =>
                                     (x.Language == request.CultureName) ||
                                     (!request.CultureName.IsNullOrEmpty() && x.Language.IsNullOrEmpty()))
                                 ?? brokenLinkResult.Results.FirstOrDefault();

                result.RedirectUrl = resultItem?.RedirectUrl;
            }
        }

        return result;
    }

    protected virtual async Task<SeoInfo> GetBestMatchingSeoInfo(SeoSearchCriteria criteria, Store store)
    {
        var itemsToMatch = await seoResolver.FindSeoAsync(criteria);
        return itemsToMatch.GetBestMatchingSeoInfo(store, criteria.LanguageCode);
    }
}
