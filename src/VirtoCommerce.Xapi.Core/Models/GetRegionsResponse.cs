using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Xapi.Core.Models
{
    public class GetRegionsResponse
    {
        public IList<CountryRegion> Regions { get; set; }
    }
}
