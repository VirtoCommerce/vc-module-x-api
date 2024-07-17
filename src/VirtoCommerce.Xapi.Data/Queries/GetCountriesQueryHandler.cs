using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Queries;

namespace VirtoCommerce.Xapi.Data.Queries
{
    public class GetCountriesQueryHandler : IQueryHandler<GetCountriesQuery, GetCountriesResponse>,
        IQueryHandler<GetRegionsQuery, GetRegionsResponse>
    {
        private readonly ICountriesService _countriesService;

        public GetCountriesQueryHandler(ICountriesService countriesService)
        {
            _countriesService = countriesService;
        }

        /// <summary>
        /// Return countries list
        /// </summary>
        public virtual async Task<GetCountriesResponse> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
        {
            var countries = await _countriesService.GetCountriesAsync();
            var result = new GetCountriesResponse { Countries = countries };

            return result;
        }

        /// <summary>
        /// Return regions of the country
        /// </summary>
        public virtual async Task<GetRegionsResponse> Handle(GetRegionsQuery request, CancellationToken cancellationToken)
        {
            var regions = await _countriesService.GetCountryRegionsAsync(request.CountryId);
            var result = new GetRegionsResponse() { Regions = regions };

            return result;
        }
    }
}
