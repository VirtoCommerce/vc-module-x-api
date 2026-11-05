using System.Collections.Generic;
using FluentAssertions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.Xapi.Core.Index;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Index;

public class IFilterExtensionsTests
{
    [Fact]
    public void MapTo_ListOfIntProperty_ReceivesConvertedValues()
    {
        var criteria = new TestCriteria();

        new TermFilter { FieldName = "numbers", Values = ["1", "2", "3"] }.MapTo(criteria);

        criteria.Numbers.Should().Equal(1, 2, 3);
    }

    [Theory]
    [InlineData("stringIList", nameof(TestCriteria.StringIList))]
    [InlineData("stringList", nameof(TestCriteria.StringList))]
    [InlineData("stringCollection", nameof(TestCriteria.StringCollection))]
    [InlineData("stringEnumerable", nameof(TestCriteria.StringEnumerable))]
    [InlineData("stringReadOnlyList", nameof(TestCriteria.StringReadOnlyList))]
    public void MapTo_ListTypedProperty_ReceivesAllValues(string fieldName, string propertyName)
    {
        var criteria = new TestCriteria();

        new TermFilter { FieldName = fieldName, Values = ["a", "b", "c"] }.MapTo(criteria);

        var values = (IEnumerable<string>)typeof(TestCriteria).GetProperty(propertyName).GetValue(criteria);
        values.Should().Equal("a", "b", "c");
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
        public IList<string> StringIList { get; set; }

        public List<string> StringList { get; set; }

        public ICollection<string> StringCollection { get; set; }

        public IEnumerable<string> StringEnumerable { get; set; }

        public IReadOnlyList<string> StringReadOnlyList { get; set; }

        public string[] Codes { get; set; }

        public string Name { get; set; }

        public IList<int> Numbers { get; set; }

        public int? Count { get; set; }
    }
}
