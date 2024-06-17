using System.Collections.Generic;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.Xapi.Core.Models
{
    public class SearchDynamicPropertyDictionaryItemResponse
    {
        public IList<DynamicPropertyDictionaryItem> Results { get; set; }
        public int TotalCount { get; set; }
    }
}
