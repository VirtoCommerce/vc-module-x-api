using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Queries
{
    public class GetRegionsQuery : IQuery<GetRegionsResponse>
    {
        public string CountryId { get; set; }
    }
}
