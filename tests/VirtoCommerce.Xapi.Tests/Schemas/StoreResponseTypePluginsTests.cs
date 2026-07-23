using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using GraphQL.Execution;
using Moq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Schemas;
using VirtoCommerce.Xapi.Core.Services;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Schemas
{
    public class StoreResponseTypePluginsTests
    {
        private readonly Mock<IAppManifestService> _appManifestService = new();
        private readonly StoreResponseType _storeResponseType;

        public StoreResponseTypePluginsTests()
        {
            _storeResponseType = new StoreResponseType(Mock.Of<IDynamicPropertyResolverService>(), _appManifestService.Object);
        }

        [Fact]
        public async Task Plugins_ShouldUseProvidedAppId()
        {
            // Arrange
            _appManifestService.Setup(x => x.GetManifest("custom-app"))
                .Returns(new AppManifestDescriptor { Plugins = [new PluginDescriptor { Id = "VirtoCommerce.Custom" }] });
            var context = new ResolveFieldContext<StoreResponse>
            {
                Source = new StoreResponse(),
                Arguments = new Dictionary<string, ArgumentValue> { ["appId"] = new ArgumentValue("custom-app", ArgumentSource.Literal) },
            };

            // Act
            var result = await ResolvePluginsAsync(context);

            // Assert
            _appManifestService.Verify(x => x.GetManifest("custom-app"), Times.Once);
            result.Should().ContainSingle().Which.Id.Should().Be("VirtoCommerce.Custom");
        }

        [Fact]
        public async Task Plugins_ShouldReturnEmptyList_WhenManifestIsNull()
        {
            // Arrange
            _appManifestService.Setup(x => x.GetManifest(It.IsAny<string>())).Returns((AppManifestDescriptor)null);
            var context = new ResolveFieldContext<StoreResponse> { Source = new StoreResponse() };

            // Act
            var result = await ResolvePluginsAsync(context);

            // Assert
            result.Should().NotBeNull().And.BeEmpty();
        }

        private async Task<IList<StorePlugin>> ResolvePluginsAsync(IResolveFieldContext<StoreResponse> context)
        {
            var field = _storeResponseType.Fields.FirstOrDefault(x => x.Name.EqualsIgnoreCase("plugins"));
            field.Should().NotBeNull();
            var result = await field.Resolver.ResolveAsync(context);
            return ((IEnumerable<StorePlugin>)result).ToList();
        }
    }
}
