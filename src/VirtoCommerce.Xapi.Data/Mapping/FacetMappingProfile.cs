using System;
using System.Linq;
using AutoMapper;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Models.Facets;

namespace VirtoCommerce.Xapi.Data.Mapping;

public class FacetMappingProfile : Profile
{
    public FacetMappingProfile()
    {
        CreateMap<Aggregation, FacetResult>().IncludeAllDerived().ConvertUsing((request, facet, context) =>
        {
            context.Items.TryGetValue("cultureName", out var cultureNameObj);
            var cultureName = cultureNameObj as string;
            FacetResult result = request.AggregationType switch
            {
                "attr" => new TermFacetResult
                {
                    Name = request.Field,
                    Label = request.Field,
                    Terms = request.Items?.Select(x => new FacetTerm
                    {
                        Count = x.Count,
                        IsSelected = x.IsApplied,
                        Term = x.Value?.ToString(),
                        Label = x.Labels?.FirstBestMatchForLanguage(x => x.Language, cultureName)?.Label ?? x.Value?.ToString(),
                    }).ToArray() ?? [],

                },
                "range" => new RangeFacetResult
                {
                    Name = request.Field,
                    Label = request.Field,
                    Ranges = request.Items?.Select(x => new FacetRange
                    {
                        Count = x.Count,
                        IsSelected = x.IsApplied,
                        From = Convert.ToInt64(x.RequestedLowerBound),
                        IncludeFrom = x.IncludeLower,
                        FromStr = x.RequestedLowerBound,
                        To = Convert.ToInt64(x.RequestedUpperBound),
                        IncludeTo = x.IncludeUpper,
                        ToStr = x.RequestedUpperBound,
                        Label = x.Value?.ToString(),
                    }).ToArray() ?? [],
                },
                _ => null
            };

            return result;
        });
    }
}
