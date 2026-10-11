using System;
using FluentAssertions;
using Moq;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using VirtoCommerce.Xapi.Data.Services;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Services;

public class DynamicPropertyDictionaryItemSearchCriteriaBuilderTests
{
    [Fact]
    public void ParseFilters_NullPhrase_LeavesCriteriaUnchanged()
    {
        var builder = new DynamicPropertyDictionaryItemSearchCriteriaBuilder(Mock.Of<ISearchPhraseParser>());

        var result = builder.ParseFilters(null).Build();

        result.PropertyId.Should().BeNull();
    }

    [Fact]
    public void ParseFilters_NoParserSet_Throws()
    {
        var builder = new DynamicPropertyDictionaryItemSearchCriteriaBuilder();

        var act = () => builder.ParseFilters("propertyId:prop-1");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ParseFilters_TermFilter_AppliesMatchingCriteriaProperty()
    {
        // Moved verbatim from the old AutoMapper profile's ConvertUsing - no AutoMapper behaviour to compare against.
        var parserMock = new Mock<ISearchPhraseParser>();
        parserMock.Setup(x => x.Parse(It.IsAny<string>())).Returns(new SearchPhraseParseResult
        {
            Filters =
            [
                new TermFilter { FieldName = "propertyId", Values = ["prop-1"] },
            ],
        });

        var builder = new DynamicPropertyDictionaryItemSearchCriteriaBuilder(parserMock.Object);

        var result = builder.ParseFilters("propertyId:prop-1").Build();

        result.PropertyId.Should().Be("prop-1");
    }
}
