using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Queries;

namespace VirtoCommerce.Xapi.Data.Queries
{
    public class SlugInfoQueryHandler : IQueryHandler<SlugInfoQuery, SlugInfoResponse>
    {
        private readonly CompositeSeoResolver _seoResolver;
        private readonly IStoreService _storeService;

        public SlugInfoQueryHandler(CompositeSeoResolver seoResolver, IStoreService storeService)
        {
            _seoResolver = seoResolver;
            _storeService = storeService;
        }

        public async Task<SlugInfoResponse> Handle(SlugInfoQuery request, CancellationToken cancellationToken)
        {
            var result = new SlugInfoResponse();

            if (string.IsNullOrEmpty(request.Permalink))
            {
                return result;
            }

            var store = await _storeService.GetByIdAsync(request.StoreId);
            if (store is null)
            {
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

            return result;
        }

        protected virtual async Task<SeoInfo> GetBestMatchingSeoInfo(SeoSearchCriteria criteria, Store store)
        {
            var itemsToMatch = await _seoResolver.FindSeoAsync(criteria);

            var seoInfosForStore = itemsToMatch.Where(x => x.StoreId == store.Id).ToArray();
            var bestMatchSeoInfo = seoInfosForStore.GetBestMatchingSeoInfo(store, criteria.LanguageCode, criteria.Slug, criteria.Permalink);

            if (bestMatchSeoInfo == null)
            {
                var seoInfosWithoutStore = itemsToMatch.Where(x => string.IsNullOrEmpty(x.StoreId)).ToArray();
                bestMatchSeoInfo = seoInfosWithoutStore.GetBestMatchingSeoInfo(store, criteria.LanguageCode, criteria.Slug, criteria.Permalink);
            }

            return bestMatchSeoInfo;
        }
    }
}
