using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Services
{
    public interface IDynamicPropertyUpdaterService
    {
        Task UpdateDynamicPropertyValues(IHasDynamicProperties entity, IList<DynamicPropertyValue> values);
    }
}
