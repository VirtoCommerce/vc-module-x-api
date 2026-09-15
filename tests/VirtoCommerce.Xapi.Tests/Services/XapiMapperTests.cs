using AutoMapper;
using FluentAssertions;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Data.Mapping;
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
        result.Type.Should().Be(source.MemberType);
        result.Rating.Should().BeNull();
    }

    [Fact]
    public void ToExpVendor_ProducesSameResultAsLegacyAutoMapperProfile()
    {
        // LegacyVendorMappingProfile is the same profile the Obsolete AutoMapper.IMapper-typed
        // DataLoaderContextAccessorExtensions overloads use in production - not a separate test double.
        var source = new Contact
        {
            Id = "vendor-2",
            Name = "Legacy Vendor",
        };

        var legacyMapper = new MapperConfiguration(cfg =>
            cfg.AddProfile<LegacyVendorMappingProfile>()).CreateMapper();

        var expected = legacyMapper.Map<ExpVendor>(source);
        var actual = _mapper.ToExpVendor(source);

        actual.Should().BeEquivalentTo(expected);
    }
}
