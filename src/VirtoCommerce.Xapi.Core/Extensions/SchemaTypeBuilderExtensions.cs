using GraphQL.Types;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Infrastructure;

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
