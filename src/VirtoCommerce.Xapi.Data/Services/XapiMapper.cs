using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Services;

namespace VirtoCommerce.Xapi.Data.Services;

public class XapiMapper : IXapiMapper
{
    public virtual ExpVendor ToExpVendor(Member source)
    {
        if (source == null)
        {
            return null;
        }

        var result = AbstractTypeFactory<ExpVendor>.TryCreateInstance();

        result.Id = source.Id;
        result.Name = source.Name;
        result.Type = source.MemberType;

        return result;
    }
}
