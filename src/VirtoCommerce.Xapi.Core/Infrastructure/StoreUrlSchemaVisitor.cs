using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Schemas.ScalarTypes;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    /// <summary>
    /// Wraps the resolver of every field declared with <see cref="StoreUrlType"/> so the
    /// resolved URL is passed through <see cref="IStoreAssetPublicUrlResolver"/> with the store taken
    /// from the GraphQL context. Declaring the field type is enough — no per-field resolver is needed.
    /// </summary>
    public class StoreUrlSchemaVisitor : BaseSchemaNodeVisitor
    {
        public override void VisitObjectFieldDefinition(FieldType field, IObjectGraphType type, ISchema schema)
        {
            if (IsStoreAssetUrlType(field.Type) || field.ResolvedType?.GetNamedType() is StoreUrlType)
            {
                field.Resolver = new StoreUrlFieldResolver(field.Resolver);
            }
        }

        private static bool IsStoreAssetUrlType(Type type)
        {
            while (type != null && type.IsGenericType)
            {
                type = type.GetGenericArguments()[0];
            }

            return type == typeof(StoreUrlType);
        }

        private sealed class StoreUrlFieldResolver : IFieldResolver
        {
            private readonly IFieldResolver _inner;

            public StoreUrlFieldResolver(IFieldResolver inner)
            {
                _inner = inner ?? NameFieldResolver.Instance;
            }

            public async ValueTask<object> ResolveAsync(IResolveFieldContext context)
            {
                var value = await _inner.ResolveAsync(context);

                if (value is string url && !string.IsNullOrEmpty(url))
                {
                    var store = context.GetArgumentOrValue<Store>("store");
                    var urlResolver = context.RequestServices?.GetService<IStoreAssetPublicUrlResolver>();

                    if (store != null && urlResolver != null)
                    {
                        return urlResolver.GetAbsoluteUrl(store, url);
                    }
                }

                return value;
            }
        }
    }
}
