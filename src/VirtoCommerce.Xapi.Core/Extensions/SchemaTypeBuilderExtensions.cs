using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Xapi.Core.Extensions
{
    public static class SchemaTypeBuilderExtensions
    {
        public static ISchemaTypeBuilder<TDerivedSchemaType> OverrideType<TBaseSchemaType, TDerivedSchemaType>(this ISchemaTypeBuilder<TDerivedSchemaType> builder) where TBaseSchemaType : IGraphType where TDerivedSchemaType : TBaseSchemaType, IGraphType
        {
            AbstractTypeFactory<IGraphType>.OverrideType<TBaseSchemaType, TDerivedSchemaType>();
            return builder;
        }
    }
}
