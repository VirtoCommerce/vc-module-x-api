using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Extensions;

namespace VirtoCommerce.Xapi.Core.Helpers
{
    public class FieldCreator
    {
        public static FieldType CreateField<TSourceType, TGraphType>(
          string name,
          string description = null,
          QueryArguments arguments = null,
          Func<IResolveFieldContext<TSourceType>, object> resolve = null,
          string deprecationReason = null)
            where TGraphType : IGraphType
        {
            return new FieldType
            {
                Name = name,
                Description = description,
                DeprecationReason = deprecationReason,
                Type = GraphTypeExtensionHelper.GetActualComplexType<TGraphType>(),
                Arguments = arguments,
                Resolver = resolve != null
                    ? new FuncFieldResolver<TSourceType, object>(context =>
                    {
                        context.CopyArgumentsToUserContext();
                        return resolve(context);
                    })
                    : null
            };
        }

        public static FieldType CreateFieldAsync<TSourceType, TGraphType>(
          string name,
          string description = null,
          QueryArguments arguments = null,
          Func<IResolveFieldContext<TSourceType>, Task<object>> resolve = null,
          string deprecationReason = null)
            where TGraphType : IGraphType
        {
            return new FieldType
            {
                Name = name,
                Description = description,
                DeprecationReason = deprecationReason,
                Type = GraphTypeExtensionHelper.GetActualComplexType<TGraphType>(),
                Arguments = arguments,
                Resolver = resolve != null
                    ? new FuncFieldResolver<TSourceType, object>(context =>
                    {
                        context.CopyArgumentsToUserContext();
                        return new ValueTask<object>(resolve(context));
                    })
                    : null
            };
        }
    }
}
