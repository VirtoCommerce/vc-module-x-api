using System.Collections.Generic;
using FluentAssertions;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Xapi.Core.Models;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Models
{
    public class StorePluginMappingTests
    {
        [Fact]
        public void FromDescriptor_ShouldMapAllFields()
        {
            // Arrange
            var descriptor = new PluginDescriptor
            {
                Id = "VirtoCommerce.Sample",
                Version = "1.2.3",
                Permission = "sample:access",
                Entry = new ContentFileDescriptor { Type = "script", Path = "/modules/$(VirtoCommerce.Sample)/plugins/vc-frontend/remoteEntry.js", Hash = "ABC123" },
                ContentFiles = new List<ContentFileDescriptor>
                {
                    new() { Type = "style", Path = "/modules/$(VirtoCommerce.Sample)/plugins/vc-frontend/style.css", Hash = "DEF456" },
                },
                Remote = new PluginRemoteDescriptor { Name = "VirtoCommerce.Sample", Exposed = "./Module" },
            };

            // Act
            var result = StorePlugin.FromDescriptor(descriptor);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("VirtoCommerce.Sample");
            result.Version.Should().Be("1.2.3");
            result.Permission.Should().Be("sample:access");
            result.Entry.Should().NotBeNull();
            result.Entry.Type.Should().Be("script");
            result.Entry.Path.Should().Be("/modules/$(VirtoCommerce.Sample)/plugins/vc-frontend/remoteEntry.js");
            result.Entry.Hash.Should().Be("ABC123");
            result.ContentFiles.Should().HaveCount(1);
            result.ContentFiles[0].Type.Should().Be("style");
            result.ContentFiles[0].Path.Should().Be("/modules/$(VirtoCommerce.Sample)/plugins/vc-frontend/style.css");
            result.Remote.Should().NotBeNull();
            result.Remote.Name.Should().Be("VirtoCommerce.Sample");
            result.Remote.Exposed.Should().Be("./Module");
        }

        [Fact]
        public void FromDescriptor_ShouldReturnNull_WhenDescriptorIsNull()
        {
            // Act & Assert
            StorePlugin.FromDescriptor(null).Should().BeNull();
        }

        [Fact]
        public void FromDescriptor_ShouldTolerateMissingOptionalParts()
        {
            // Arrange
            var descriptor = new PluginDescriptor
            {
                Id = "VirtoCommerce.Minimal",
                Version = "1.0.0",
                // No Entry, no ContentFiles, no Remote, no Permission.
                ContentFiles = null,
            };

            // Act
            var result = StorePlugin.FromDescriptor(descriptor);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("VirtoCommerce.Minimal");
            result.Entry.Should().BeNull();
            result.Remote.Should().BeNull();
            result.Permission.Should().BeNull();
            result.ContentFiles.Should().NotBeNull().And.BeEmpty();
        }
    }
}
