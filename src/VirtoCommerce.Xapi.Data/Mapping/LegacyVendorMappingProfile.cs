using AutoMapper;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Data.Services;

namespace VirtoCommerce.Xapi.Data.Mapping;

/// <summary>
/// The only AutoMapper profile x-api still ships. Exists purely so the Obsolete
/// <c>AutoMapper.IMapper</c>-typed <c>DataLoaderContextAccessorExtensions.GetVendorDataLoader</c>/
/// <c>LoadVendor</c> overloads keep working for modules that haven't moved to <see cref="Core.Services.IXapiMapper"/>
/// yet. Delegates to <see cref="XapiMapper"/> rather than re-implementing the conversion, so there is
/// exactly one place that owns the <c>Member -&gt; ExpVendor</c> mapping. Do not add more maps here.
/// </summary>
public class LegacyVendorMappingProfile : Profile
{
    public LegacyVendorMappingProfile()
    {
        var mapper = new XapiMapper();

        CreateMap<Member, ExpVendor>().ConvertUsing((src, _) => mapper.ToExpVendor(src));
    }
}
