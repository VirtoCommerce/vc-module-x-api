using AutoMapper;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Tests.Services;

/// <summary>
/// The original AutoMapper profile this replaced, kept as the test-only parity oracle for
/// <see cref="Data.Services.XapiMapper"/>'s <c>Member -&gt; ExpVendor</c> conversion - not shipped,
/// per this wave's convention (see <c>FacetMappingProfile</c>).
/// </summary>
public class LegacyXapiMappingProfile : Profile
{
    public LegacyXapiMappingProfile()
    {
        CreateMap<Member, ExpVendor>().ConvertUsing((src, _) =>
        {
            var result = AbstractTypeFactory<ExpVendor>.TryCreateInstance();
            result.Id = src.Id;
            result.Name = src.Name;
            result.Type = src.MemberType;
            return result;
        });
    }
}
