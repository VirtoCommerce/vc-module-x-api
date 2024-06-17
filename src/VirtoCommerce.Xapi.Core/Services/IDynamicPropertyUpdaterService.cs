using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.Xapi.Core.Services
{
    public interface IDynamicPropertyUpdaterService
    {
        Task UpdateDynamicPropertyValues(IHasDynamicProperties entity, IList<DynamicPropertyValue> values);
    }
}
