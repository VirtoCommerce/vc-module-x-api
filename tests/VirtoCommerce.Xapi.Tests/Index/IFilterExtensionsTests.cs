using System.Collections.Generic;
using FluentAssertions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.Xapi.Core.Index;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Index;

public class IFilterExtensionsTests
{
    [Fact]
    public void MapTo_ListOfStringProperty_ReceivesAllValues()
    {
        var criteria = new TestCriteria();

        new TermFilter { FieldName = "tags", Values = ["a", "b", "c"] }.MapTo(criteria);

        criteria.Tags.Should().Equal("a", "b", "c");
    }

    [Fact]
    public void MapTo_ListOfIntProperty_ReceivesConvertedValues()
    {
        var criteria = new TestCriteria();

        new TermFilter { FieldName = "numbers", Values = ["1", "2", "3"] }.MapTo(criteria);

        criteria.Numbers.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void MapTo_ArrayProperty_ReceivesAllValues()
    {
        var criteria = new TestCriteria();

        new TermFilter { FieldName = "codes", Values = ["x", "y"] }.MapTo(criteria);

        criteria.Codes.Should().Equal("x", "y");
    }

    [Fact]
    public void MapTo_StringProperty_ReceivesFirstValue()
    {
        var criteria = new TestCriteria();

        new TermFilter { FieldName = "name", Values = ["first", "second"] }.MapTo(criteria);

        criteria.Name.Should().Be("first");
    }

    [Fact]
    public void MapTo_NullableIntProperty_ReceivesFirstValue()
    {
        var criteria = new TestCriteria();

        new TermFilter { FieldName = "count", Values = ["7", "8"] }.MapTo(criteria);

        criteria.Count.Should().Be(7);
    }

    private sealed class TestCriteria
    {
        public IList<string> Tags { get; set; }

        public string[] Codes { get; set; }

        public string Name { get; set; }

        public IList<int> Numbers { get; set; }

        public int? Count { get; set; }
    }
}
