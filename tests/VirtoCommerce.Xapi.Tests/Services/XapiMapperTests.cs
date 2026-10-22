using FluentAssertions;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Data.Services;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Services;

public class XapiMapperTests
{
    private readonly XapiMapper _mapper = new();

    [Fact]
    public void ToExpVendor_NullSource_ReturnsNull()
    {
        _mapper.ToExpVendor(null).Should().BeNull();
    }

    [Fact]
    public void ToExpVendor_MapsIdNameAndType()
    {
        var source = new Contact
        {
            Id = "vendor-1",
            Name = "Acme Corp",
        };

        var result = _mapper.ToExpVendor(source);

        result.Should().NotBeNull();
        result.Id.Should().Be("vendor-1");
        result.Name.Should().Be("Acme Corp");
        result.Type.Should().Be("Contact");
        result.Rating.Should().BeNull();
    }
}
