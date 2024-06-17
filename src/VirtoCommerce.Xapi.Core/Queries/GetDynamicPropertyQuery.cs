using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Queries
{
    public class GetDynamicPropertyQuery : IDynamicPropertiesQuery, IQuery<GetDynamicPropertyResponse>
    {
        public string IdOrName { get; set; }
        public string CultureName { get; set; }
        public string ObjectType { get; set; }
    }
}
