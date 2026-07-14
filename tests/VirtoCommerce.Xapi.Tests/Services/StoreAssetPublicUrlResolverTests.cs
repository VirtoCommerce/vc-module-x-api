using FluentAssertions;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.Xapi.Data.Services;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Services
{
    public class StoreAssetPublicUrlResolverTests
    {
        private readonly StoreAssetPublicUrlResolver _resolver = new();

        private static Store StoreWith(string assetPublicUrl) => new() { AssetPublicUrl = assetPublicUrl };

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetAbsoluteUrl_NullOrEmpty_ReturnsAsIs(string url)
        {
            //Arrange
            var store = StoreWith("https://cdn.store1.com");

            //Act
            var result = _resolver.GetAbsoluteUrl(store, url);

            //Assert
            result.Should().Be(url);
        }

        [Theory]
        [InlineData("data:image/png;base64,iVBORw0KGgo=")]
        [InlineData("blob:https://x/9b2c")]
        public void GetAbsoluteUrl_DataOrBlobScheme_ReturnsAsIs(string url)
        {
            //Arrange
            var store = StoreWith("https://cdn.store1.com");

            //Act
            var result = _resolver.GetAbsoluteUrl(store, url);

            //Assert
            result.Should().Be(url);
        }

        [Theory]
        [InlineData("catalog/x.jpg")]
        [InlineData("https://global.example.com/assets/catalog/x.jpg")]
        public void GetAbsoluteUrl_NoStoreAssetUrl_ReturnsAsIs(string url)
        {
            //Arrange
            var store = StoreWith(null);

            //Act
            var result = _resolver.GetAbsoluteUrl(store, url);

            //Assert
            result.Should().Be(url);
        }

        [Fact]
        public void GetAbsoluteUrl_NullStore_ThrowsArgumentNullException()
        {
            //Act
            var action = () => _resolver.GetAbsoluteUrl(null, "catalog/x.jpg");

            //Assert
            action.Should().Throw<System.ArgumentNullException>();
        }

        [Theory]
        [InlineData("catalog/x.jpg")]
        [InlineData("/catalog/x.jpg")]
        public void GetAbsoluteUrl_RelativeUrl_CombinesWithStoreAssetUrl(string url)
        {
            //Arrange
            var store = StoreWith("https://cdn.store1.com");

            //Act
            var result = _resolver.GetAbsoluteUrl(store, url);

            //Assert
            result.Should().Be("https://cdn.store1.com/catalog/x.jpg");
        }

        [Fact]
        public void GetAbsoluteUrl_RelativeUrl_TrailingSlashBase_CombinesWithoutDoubleSlash()
        {
            //Arrange
            var store = StoreWith("https://cdn.store1.com/assets/");

            //Act
            var result = _resolver.GetAbsoluteUrl(store, "catalog/x.jpg");

            //Assert
            result.Should().Be("https://cdn.store1.com/assets/catalog/x.jpg");
        }

        [Fact]
        public void GetAbsoluteUrl_AbsoluteUrl_RebasesHostToStoreAssetUrl()
        {
            //Arrange
            var store = StoreWith("https://cdn.store1.com");

            //Act
            var result = _resolver.GetAbsoluteUrl(store, "https://global.example.com/assets/catalog/x.jpg");

            //Assert
            result.Should().Be("https://cdn.store1.com/assets/catalog/x.jpg");
        }

        [Fact]
        public void GetAbsoluteUrl_BareHostStoreAssetUrl_NormalizesToHttps()
        {
            //Arrange
            var store = StoreWith("cdn.store1.com");

            //Act
            var result = _resolver.GetAbsoluteUrl(store, "https://global.example.com/assets/catalog/x.jpg");

            //Assert
            result.Should().Be("https://cdn.store1.com/assets/catalog/x.jpg");
        }

        [Fact]
        public void GetAbsoluteUrl_StoreAssetUrlWithBasePath_PrependsPathOnRebase()
        {
            //Arrange
            var store = StoreWith("https://cdn.com/tenant1");

            //Act
            var result = _resolver.GetAbsoluteUrl(store, "https://global.example.com/assets/catalog/x.jpg");

            //Assert
            result.Should().Be("https://cdn.com/tenant1/assets/catalog/x.jpg");
        }

        [Fact]
        public void GetAbsoluteUrl_AbsoluteUrlWithPort_RebasesToStoreDefaultPort()
        {
            //Arrange
            var store = StoreWith("https://cdn.store1.com");

            //Act
            var result = _resolver.GetAbsoluteUrl(store, "http://localhost:10645/assets/catalog/x.jpg");

            //Assert
            result.Should().Be("https://cdn.store1.com/assets/catalog/x.jpg");
        }

        [Theory]
        [InlineData("https://global.example.com/assets/x.jpg?sas=abc#frag", "https://cdn.store1.com/assets/x.jpg?sas=abc#frag")]
        [InlineData("catalog/x.jpg?sas=abc#frag", "https://cdn.store1.com/catalog/x.jpg?sas=abc#frag")]
        public void GetAbsoluteUrl_PreservesQueryAndFragment(string url, string expected)
        {
            //Arrange
            var store = StoreWith("https://cdn.store1.com");

            //Act
            var result = _resolver.GetAbsoluteUrl(store, url);

            //Assert
            result.Should().Be(expected);
        }
    }
}
