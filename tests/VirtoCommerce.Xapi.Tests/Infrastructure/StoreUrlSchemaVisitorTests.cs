using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Schemas.ScalarTypes;
using VirtoCommerce.Xapi.Core.Services;
using VirtoCommerce.Xapi.Data.Services;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Infrastructure
{
    public class StoreUrlSchemaVisitorTests
    {
        private readonly StoreUrlSchemaVisitor _visitor = new();

        private static FieldType CreateField(System.Type graphType, string resolvedValue)
        {
            return new FieldType
            {
                Name = "url",
                Type = graphType,
                Resolver = new FuncFieldResolver<object>(_ => resolvedValue),
            };
        }

        private static ResolveFieldContext CreateContext(Store store)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IStoreAssetPublicUrlResolver, StoreAssetPublicUrlResolver>();

            var userContext = new Dictionary<string, object>();
            if (store != null)
            {
                userContext["store"] = store;
            }

            return new ResolveFieldContext
            {
                UserContext = userContext,
                RequestServices = services.BuildServiceProvider(),
            };
        }

        [Theory]
        [InlineData(typeof(StoreUrlType))]
        [InlineData(typeof(NonNullGraphType<StoreUrlType>))]
        public async Task Visit_StoreAssetUrlField_WrapsResolverAndAppliesStoreAssetUrl(System.Type graphType)
        {
            //Arrange
            var field = CreateField(graphType, "catalog/x.jpg");
            var originalResolver = field.Resolver;

            //Act
            _visitor.VisitObjectFieldDefinition(field, type: null, schema: null);
            var result = await field.Resolver.ResolveAsync(CreateContext(new Store { AssetPublicUrl = "https://cdn.store1.com" }));

            //Assert
            field.Resolver.Should().NotBeSameAs(originalResolver);
            result.Should().Be("https://cdn.store1.com/catalog/x.jpg");
        }

        [Fact]
        public void Visit_PlainStringField_DoesNotWrapResolver()
        {
            //Arrange
            var field = CreateField(typeof(NonNullGraphType<StringGraphType>), "catalog/x.jpg");
            var originalResolver = field.Resolver;

            //Act
            _visitor.VisitObjectFieldDefinition(field, type: null, schema: null);

            //Assert
            field.Resolver.Should().BeSameAs(originalResolver);
        }

        [Fact]
        public async Task Resolve_NoStoreInContext_ReturnsOriginalValue()
        {
            //Arrange
            var field = CreateField(typeof(StoreUrlType), "catalog/x.jpg");
            _visitor.VisitObjectFieldDefinition(field, type: null, schema: null);

            //Act
            var result = await field.Resolver.ResolveAsync(CreateContext(store: null));

            //Assert
            result.Should().Be("catalog/x.jpg");
        }

        [Fact]
        public async Task Resolve_NullValue_ReturnsNull()
        {
            //Arrange
            var field = CreateField(typeof(StoreUrlType), resolvedValue: null);
            _visitor.VisitObjectFieldDefinition(field, type: null, schema: null);

            //Act
            var result = await field.Resolver.ResolveAsync(CreateContext(new Store { AssetPublicUrl = "https://cdn.store1.com" }));

            //Assert
            result.Should().BeNull();
        }
    }
}
