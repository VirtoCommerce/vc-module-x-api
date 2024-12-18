using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class ExtendableInputGraphType : ExtendableInputGraphType<object>
    {
    }

    public class ExtendableInputGraphType<TSourceType> : InputObjectGraphType<TSourceType>
    {
        public new FieldType Field<TGraphType>(
            string name,
            string description = null,
            QueryArguments arguments = null,
            Func<IResolveFieldContext<TSourceType>, object> resolve = null,
            string deprecationReason = null)
            where TGraphType : IGraphType
        {
            return AddField(new FieldType
            {
                Name = name,
                Description = description,
                DeprecationReason = deprecationReason,
                Type = typeof(TGraphType),
                Arguments = arguments,
                Resolver = resolve != null
                    ? new FuncFieldResolver<TSourceType, object>(resolve)
                    : null
            });
        }

        public new FieldType FieldAsync<TGraphType>(
            string name,
            string description = null,
            QueryArguments arguments = null,
            Func<IResolveFieldContext<TSourceType>, Task<object>> resolve = null,
            string deprecationReason = null)
            where TGraphType : IGraphType
        {
            return AddField(new FieldType
            {
                Name = name,
                Description = description,
                DeprecationReason = deprecationReason,
                Type = typeof(TGraphType),
                Arguments = arguments,
                Resolver = resolve != null
                    ? new FuncFieldResolver<TSourceType, object>(context => new ValueTask<object>(resolve(context)))
                    : null
            });
        }
    }
}
