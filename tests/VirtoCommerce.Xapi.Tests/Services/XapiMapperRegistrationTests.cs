using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Xapi.Core.Services;
using VirtoCommerce.Xapi.Data.Extensions;
using VirtoCommerce.Xapi.Data.Services;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Services;

public class XapiMapperRegistrationTests
{
    [Fact]
    public void AddXCore_RegistersXapiMapperAsSingleton()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddXCore(configuration);

        var descriptor = services.SingleOrDefault(x => x.ServiceType == typeof(IXapiMapper));

        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(XapiMapper));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }
}
