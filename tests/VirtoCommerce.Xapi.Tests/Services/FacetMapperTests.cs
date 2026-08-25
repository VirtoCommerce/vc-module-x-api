using System;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.Xapi.Core.Models.Facets;
using VirtoCommerce.Xapi.Data.Mapping;
using VirtoCommerce.Xapi.Data.Services;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Services;

public class FacetMapperTests
{
    private readonly FacetMapper _mapper = new();

    private static readonly IMapper _legacyMapper = new MapperConfiguration(cfg =>
        cfg.AddProfile<FacetMappingProfile>()).CreateMapper();

    [Fact]
    public void ToFacetResult_NullSource_ReturnsNull()
    {
        _mapper.ToFacetResult(null, new FacetMappingContext { CultureName = "en-US" }).Should().BeNull();
    }

    [Fact]
    public void CreateFacetMappingContext_PopulatesCultureName_ViaAbstractTypeFactory()
    {
        var context = _mapper.CreateFacetMappingContext("en-US");

        context.Should().NotBeNull();
        context.CultureName.Should().Be("en-US");
    }

    [Fact]
    public void ToFacetResult_NullContext_FallsBackToRawValuesWithoutThrowing()
    {
        // Null context means "no localization available", not an error - labels fall back to raw values.
        var source = new AggregationFacetSource
        {
            AggregationType = "attr",
            Field = "color",
            Labels = [new AggregationFacetLabel { Language = "en-US", Label = "Color" }],
            Items =
            [
                new AggregationFacetItem
                {
                    Value = "red",
                    Count = 5,
                    IsApplied = true,
                    Labels = [new AggregationFacetLabel { Language = "en-US", Label = "Red" }],
                },
            ],
        };

        var result = _mapper.ToFacetResult(source, null) as TermFacetResult;

        result.Should().NotBeNull();
        result!.Name.Should().Be("color");
        result.Label.Should().Be("color");
        result.Terms.Should().ContainSingle();
        result.Terms[0].Label.Should().Be("red");
    }

    [Fact]
    public void ToFacetResult_UnrecognizedAggregationType_ReturnsNull()
    {
        var source = new AggregationFacetSource { AggregationType = "category", Field = "categoryId" };

        _mapper.ToFacetResult(source, new FacetMappingContext { CultureName = "en-US" }).Should().BeNull();
    }

    [Fact]
    public void ToFacetResult_AttrAggregation_MapsToTermFacetResult()
    {
        var source = new AggregationFacetSource
        {
            AggregationType = "attr",
            Field = "color",
            Labels = [new AggregationFacetLabel { Language = "en-US", Label = "Color" }],
            Items =
            [
                new AggregationFacetItem
                {
                    Value = "red",
                    Count = 5,
                    IsApplied = true,
                    Labels = [new AggregationFacetLabel { Language = "en-US", Label = "Red" }],
                },
            ],
        };

        var result = _mapper.ToFacetResult(source, new FacetMappingContext { CultureName = "en-US" }) as TermFacetResult;

        result.Should().NotBeNull();
        result!.Name.Should().Be("color");
        result.Label.Should().Be("Color");
        result.Terms.Should().HaveCount(1);
        result.Terms[0].Term.Should().Be("red");
        result.Terms[0].Label.Should().Be("Red");
        result.Terms[0].Count.Should().Be(5);
        result.Terms[0].IsSelected.Should().BeTrue();
    }

    [Fact]
    public void ToFacetResult_RangeAggregation_MapsToRangeFacetResult()
    {
        var source = new AggregationFacetSource
        {
            AggregationType = "range",
            Field = "price",
            Statistics = new AggregationFacetStatistics { Min = 1.5, Max = 99.5 },
            Items =
            [
                new AggregationFacetItem
                {
                    Value = "1-10",
                    Count = 3,
                    IsApplied = false,
                    RequestedLowerBound = "1",
                    RequestedUpperBound = "10",
                    IncludeLower = true,
                    IncludeUpper = false,
                },
            ],
        };

        var result = _mapper.ToFacetResult(source, new FacetMappingContext { CultureName = "en-US" }) as RangeFacetResult;

        result.Should().NotBeNull();
        result!.Name.Should().Be("price");
        result.Statistics.Min.Should().Be(1.5);
        result.Statistics.Max.Should().Be(99.5);
        result.Ranges.Should().HaveCount(1);
        result.Ranges[0].From.Should().Be(1);
        result.Ranges[0].To.Should().Be(10);
        result.Ranges[0].IncludeFrom.Should().BeTrue();
        result.Ranges[0].IncludeTo.Should().BeFalse();
        result.Ranges[0].Count.Should().Be(3);
    }

    [Fact]
    public void ToFacetResult_AttrAggregation_NullSortingType_LeavesOriginalOrder()
    {
        // The original AutoMapper profile never reordered terms - sorting is x-catalog's own addition,
        // opted into via TermValuesSortingType. x-order/x-pickup (no sorting type set) stay untouched.
        var source = new AggregationFacetSource
        {
            AggregationType = "attr",
            Field = "color",
            Items =
            [
                new AggregationFacetItem { Value = "b", Count = 1 },
                new AggregationFacetItem { Value = "a", Count = 2 },
                new AggregationFacetItem { Value = "c", Count = 3 },
            ],
        };

        var result = _mapper.ToFacetResult(source, new FacetMappingContext()) as TermFacetResult;

        result!.Terms.Select(x => x.Term).Should().Equal("b", "a", "c");
    }

    [Fact]
    public void ToFacetResult_AttrAggregation_NameAscending_OrdersTermsByLabelAscending()
    {
        var source = new AggregationFacetSource
        {
            AggregationType = "attr",
            Field = "color",
            TermValuesSortingType = "NameAscending",
            Items =
            [
                new AggregationFacetItem { Value = "b", Count = 1 },
                new AggregationFacetItem { Value = "a", Count = 2 },
                new AggregationFacetItem { Value = "c", Count = 3 },
            ],
        };

        var result = _mapper.ToFacetResult(source, new FacetMappingContext()) as TermFacetResult;

        result!.Terms.Select(x => x.Term).Should().Equal("a", "b", "c");
    }

    [Fact]
    public void ToFacetResult_AttrAggregation_NameDescending_OrdersTermsByLabelDescending()
    {
        var source = new AggregationFacetSource
        {
            AggregationType = "attr",
            Field = "color",
            TermValuesSortingType = "NameDescending",
            Items =
            [
                new AggregationFacetItem { Value = "b", Count = 1 },
                new AggregationFacetItem { Value = "a", Count = 2 },
                new AggregationFacetItem { Value = "c", Count = 3 },
            ],
        };

        var result = _mapper.ToFacetResult(source, new FacetMappingContext()) as TermFacetResult;

        result!.Terms.Select(x => x.Term).Should().Equal("c", "b", "a");
    }

    [Fact]
    public void ToFacetResult_RangeAggregation_EmptyLowerBound_ThrowsFormatException()
    {
        var source = new AggregationFacetSource
        {
            AggregationType = "range",
            Field = "price",
            Items = [new AggregationFacetItem { RequestedLowerBound = "", RequestedUpperBound = "100" }],
        };

        var act = () => _mapper.ToFacetResult(source, new FacetMappingContext { CultureName = "en-US" });

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ToFacetResult_AttrAggregation_ProducesSameResultAsLegacyAutoMapperProfile()
    {
        var source = new Aggregation
        {
            AggregationType = "attr",
            Field = "color",
            Labels = [new AggregationLabel { Language = "en-US", Label = "Color" }],
            Items =
            [
                new AggregationItem
                {
                    Value = "red",
                    Count = 5,
                    IsApplied = true,
                    Labels = [new AggregationLabel { Language = "en-US", Label = "Red" }],
                },
                new AggregationItem
                {
                    Value = "blue",
                    Count = 2,
                    IsApplied = false,
                },
            ],
        };

        var expected = _legacyMapper.Map<FacetResult>(source, options => options.Items["cultureName"] = "en-US");

        var actual = _mapper.ToFacetResult(ToAggregationFacetSource(source), new FacetMappingContext { CultureName = "en-US" });

        actual.Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());
    }

    [Fact]
    public void ToFacetResult_RangeAggregation_ProducesSameResultAsLegacyAutoMapperProfile()
    {
        var source = new Aggregation
        {
            AggregationType = "range",
            Field = "price",
            Statistics = new AggregationStatistics { Min = 1.5, Max = 99.5 },
            Items =
            [
                new AggregationItem
                {
                    Value = "1-10",
                    Count = 3,
                    IsApplied = false,
                    RequestedLowerBound = "1",
                    RequestedUpperBound = "10",
                    IncludeLower = true,
                    IncludeUpper = false,
                },
                new AggregationItem
                {
                    Value = "TO-100",
                    Count = 7,
                    IsApplied = false,
                    RequestedLowerBound = null,
                    RequestedUpperBound = "100",
                    IncludeLower = true,
                    IncludeUpper = false,
                },
            ],
        };

        var expected = _legacyMapper.Map<FacetResult>(source, options => options.Items["cultureName"] = "en-US");

        var actual = _mapper.ToFacetResult(ToAggregationFacetSource(source), new FacetMappingContext { CultureName = "en-US" });

        actual.Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());
    }

    private static AggregationFacetSource ToAggregationFacetSource(Aggregation source)
    {
        return new AggregationFacetSource
        {
            AggregationType = source.AggregationType,
            Field = source.Field,
            Labels = source.Labels?.Select(x => new AggregationFacetLabel { Language = x.Language, Label = x.Label }).ToList(),
            Items = source.Items?.Select(x => new AggregationFacetItem
            {
                Value = x.Value,
                Count = x.Count,
                IsApplied = x.IsApplied,
                Labels = x.Labels?.Select(l => new AggregationFacetLabel { Language = l.Language, Label = l.Label }).ToList(),
                RequestedLowerBound = x.RequestedLowerBound,
                RequestedUpperBound = x.RequestedUpperBound,
                IncludeLower = x.IncludeLower,
                IncludeUpper = x.IncludeUpper,
            }).ToList(),
            Statistics = source.Statistics == null ? null : new AggregationFacetStatistics { Min = source.Statistics.Min, Max = source.Statistics.Max },
        };
    }
}
