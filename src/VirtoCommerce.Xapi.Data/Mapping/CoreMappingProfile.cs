using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.Xapi.Core.Index;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Data.Mapping
{
    public class CoreMappingProfile : Profile
    {
        public CoreMappingProfile()
        {
            CreateMap<IList<IFilter>, DynamicPropertySearchCriteria>()
              .ConvertUsing((terms, criteria, context) =>
              {
                  foreach (var term in terms.OfType<TermFilter>())
                  {
                      term.MapTo(criteria);
                  }

                  return criteria;
              });

            CreateMap<IList<IFilter>, DynamicPropertyDictionaryItemSearchCriteria>()
              .ConvertUsing((terms, criteria, context) =>
              {
                  foreach (var term in terms.OfType<TermFilter>())
                  {
                      term.MapTo(criteria);
                  }

                  return criteria;
              });

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
}
