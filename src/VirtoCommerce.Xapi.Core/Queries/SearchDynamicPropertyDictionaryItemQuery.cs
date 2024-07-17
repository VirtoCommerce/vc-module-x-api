using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Queries
{
    public class SearchDynamicPropertyDictionaryItemQuery : IDynamicPropertiesQuery, IQuery<SearchDynamicPropertyDictionaryItemResponse>
    {
        public string PropertyId { get; set; }
        public string CultureName { get; set; }
        public string Filter { get; set; }
        public string Sort { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
