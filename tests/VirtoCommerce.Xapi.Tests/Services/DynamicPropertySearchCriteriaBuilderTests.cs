using System;
using FluentAssertions;
using Moq;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using VirtoCommerce.Xapi.Data.Services;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Services;

public class DynamicPropertySearchCriteriaBuilderTests
{
    [Fact]
    public void ParseFilters_NullPhrase_LeavesCriteriaUnchanged()
    {
        var builder = new DynamicPropertySearchCriteriaBuilder(Mock.Of<ISearchPhraseParser>());

        var result = builder.ParseFilters(null).Build();

        result.Keyword.Should().BeNull();
    }

    [Fact]
    public void ParseFilters_NoParserSet_Throws()
    {
        var builder = new DynamicPropertySearchCriteriaBuilder();

        var act = () => builder.ParseFilters("keyword:test");

        act.Should().Throw<OperationCanceledException>();
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
                new TermFilter { FieldName = "keyword", Values = ["red"] },
            ],
        });

        var builder = new DynamicPropertySearchCriteriaBuilder(parserMock.Object);

        var result = builder.ParseFilters("keyword:red").Build();

        result.Keyword.Should().Be("red");
    }
}
