using System;
using System.Globalization;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Models.Facets;
using VirtoCommerce.Xapi.Core.Services;

namespace VirtoCommerce.Xapi.Data.Services;

public class FacetMapper : IFacetMapper
{
    private const string TermValuesSortingTypeNameAscending = "NameAscending";
    private const string TermValuesSortingTypeNameDescending = "NameDescending";

    /// <summary>
    /// <paramref name="context"/> is optional: a null context means no localization is available, not
    /// an error - term/result labels fall back to the raw aggregation value/name instead of throwing.
    /// </summary>
    public virtual FacetResult ToFacetResult(AggregationFacetSource source, FacetMappingContext context)
    {
        if (source == null)
        {
            return null;
        }

        var result = CreateFacetResultByAggregationType(source, context);
        if (result == null)
        {
            return null;
        }

        result.Name = source.Field;
        result.Label = source.Labels?.FirstBestMatchForLanguage(x => x.Language, context?.CultureName)?.Label ?? result.Name;

        SortTermFacetResultByLabels(source, result, context);

        return result;
    }

    protected virtual FacetResult CreateFacetResultByAggregationType(AggregationFacetSource source, FacetMappingContext context)
    {
        return source.AggregationType switch
        {
            "attr" => ToTermFacetResult(source, context),
            "range" or "pricerange" => ToRangeFacetResult(source, context),
            _ => null,
        };
    }

    protected virtual TermFacetResult ToTermFacetResult(AggregationFacetSource source, FacetMappingContext context)
    {
        var result = AbstractTypeFactory<TermFacetResult>.TryCreateInstance();

        result.Terms = source.Items?.Select(x => ToFacetTerm(x, context)).ToArray() ?? [];

        return result;
    }

    protected virtual FacetTerm ToFacetTerm(AggregationFacetItem source, FacetMappingContext context)
    {
        var result = AbstractTypeFactory<FacetTerm>.TryCreateInstance();

        result.Count = source.Count;
        result.IsSelected = source.IsApplied;
        result.Term = source.Value?.ToString();
        result.Label = source.Labels?.FirstBestMatchForLanguage(x => x.Language, context?.CultureName)?.Label ?? source.Value?.ToString();

        return result;
    }

    protected virtual RangeFacetResult ToRangeFacetResult(AggregationFacetSource source, FacetMappingContext context)
    {
        var result = AbstractTypeFactory<RangeFacetResult>.TryCreateInstance();

        result.Ranges = source.Items?.Select(x => ToFacetRange(x, context)).ToArray() ?? [];
        result.Statistics = source.Statistics == null ? null : ToRangeFacetStatistics(source.Statistics, context);

        return result;
    }

    protected virtual FacetRange ToFacetRange(AggregationFacetItem source, FacetMappingContext context)
    {
        var result = AbstractTypeFactory<FacetRange>.TryCreateInstance();

        result.Count = source.Count;
        result.IsSelected = source.IsApplied;
        result.From = ToNullableDecimal(source.RequestedLowerBound);
        result.IncludeFrom = source.IncludeLower;
        result.FromStr = source.RequestedLowerBound;
        result.To = ToNullableDecimal(source.RequestedUpperBound);
        result.IncludeTo = source.IncludeUpper;
        result.ToStr = source.RequestedUpperBound;
        result.Label = source.Value?.ToString();

        return result;
    }

    /// <summary>
    /// The still-live AutoMapper profile parses with <c>Convert.ToInt64</c>, which throws on a null,
    /// empty, or fractional bound; VCST-2608 fixed that for x-catalog, and this is that fix, not the
    /// profile's behaviour.
    /// </summary>
    protected virtual decimal? ToNullableDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
    }

    protected virtual RangeFacetStatistics ToRangeFacetStatistics(AggregationFacetStatistics source, FacetMappingContext context)
    {
        var result = AbstractTypeFactory<RangeFacetStatistics>.TryCreateInstance();

        result.Max = source.Max;
        result.Min = source.Min;

        return result;
    }

    /// <summary>
    /// Not part of the original AutoMapper profile (which never reordered terms) - this is x-catalog's
    /// own historical behavior. A null <see cref="AggregationFacetSource.TermValuesSortingType"/> is
    /// left unsorted; callers wanting x-catalog's "null means ascending" default must set it explicitly.
    /// </summary>
    protected virtual void SortTermFacetResultByLabels(AggregationFacetSource source, FacetResult result, FacetMappingContext context)
    {
        if (result is not TermFacetResult termFacetResult || termFacetResult.Terms.IsNullOrEmpty())
        {
            return;
        }

        if (source.TermValuesSortingType.EqualsIgnoreCase(TermValuesSortingTypeNameAscending))
        {
            termFacetResult.Terms = termFacetResult.Terms.OrderBy(x => x.Label).ToArray();
        }
        else if (source.TermValuesSortingType.EqualsIgnoreCase(TermValuesSortingTypeNameDescending))
        {
            termFacetResult.Terms = termFacetResult.Terms.OrderByDescending(x => x.Label).ToArray();
        }
    }
}
